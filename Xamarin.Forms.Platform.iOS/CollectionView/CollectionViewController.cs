﻿using System;
using System.Collections;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/06/01 14:17:00 Implement Dispose override ?	
	// TODO hartez 2018/06/01 14:21:24 Add a method for updating the layout	
	internal class CollectionViewController : UICollectionViewController
	{
		readonly IEnumerable _itemsSource;
		readonly ItemsViewLayout _layout;
		readonly ItemsView _itemsView;
		bool _initialEstimateMade;

		public CollectionViewController(IEnumerable itemsSource, ItemsViewLayout layout, ItemsView itemsView) : base(layout)
		{
			_itemsSource = itemsSource;
			_layout = layout;
			_itemsView = itemsView;
		}

		void RegisterCells()
		{
			CollectionView.RegisterClassForCell(typeof(DefaultHorizontalListCell), DefaultHorizontalListCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(DefaultVerticalListCell), DefaultVerticalListCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(TemplatedHorizontalListCell), TemplatedHorizontalListCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(TemplatedVerticalListCell), TemplatedVerticalListCell.ReuseId);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			AutomaticallyAdjustsScrollViewInsets = false;
			RegisterCells();
			CollectionView.WeakDelegate = _layout;

			// TODO hartez 2018/06/10 14:29:22 Does ItemsSize need to be set at all when we're doing auto layout?
			// If not, can we leave it default/-1,-1 until UniformSize is set?	
			_layout.ItemSize = new CGSize(64, 64);
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// We can't set this constraint up on ViewDidLoad, because Forms does other stuff that resizes the view
			// and we end up with massive layout errors. And View[Will/Did]Appear do not fire for this controller
			// reliably. So until one of those options is cleared up, we set this flag so that the initial constraints
			// are set up the first time this method is called.
			if (!_initialEstimateMade)
			{
				_layout.ConstrainTo(CollectionView.Bounds.Size);
				_initialEstimateMade = true;
			}
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			// TODO hartez 2018/06/07 17:06:18 Obviously this needs to handle things which are not ILists	
			return (_itemsSource as IList).Count;
		}

		string DetermineCellReusedId()
		{
			if (_itemsView.ItemTemplate != null)
			{
				return _layout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
					? TemplatedHorizontalListCell.ReuseId
					: TemplatedVerticalListCell.ReuseId;
			}

			return _layout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? DefaultHorizontalListCell.ReuseId
				: DefaultVerticalListCell.ReuseId;
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell(DetermineCellReusedId(), indexPath) as UICollectionViewCell;

			switch (cell)
			{
				case DefaultCell defaultCell:
					UpdateDefaultCell(defaultCell, indexPath);
					break;
				case TemplatedCell templatedCell:
					UpdateTemplatedCell(templatedCell, indexPath);
					break;
			}

			return cell;
		}

		protected virtual void UpdateDefaultCell(DefaultCell defaultCell, NSIndexPath indexPath)
		{
			if (defaultCell is IConstrainedCell constrainedCell)
			{
				_layout.PrepareCellForLayout(constrainedCell);
			}

			if (_itemsSource is IList list)
			{
				defaultCell.Label.Text = list[indexPath.Row].ToString();
			}
		}

		protected virtual void UpdateTemplatedCell(TemplatedCell cell, NSIndexPath indexPath)
		{
			IVisualElementRenderer renderer = null;

			if (cell.VisualElementRenderer == null)
			{
				// We need to create a renderer, which means we need a template
				var templateElement = _itemsView.ItemTemplate.CreateContent() as View;
				renderer = CreateRenderer(templateElement);
			}

			if (_itemsSource is IList list && renderer != null)
			{
				BindableObject.SetInheritedBindingContext(renderer.Element, list[indexPath.Row]);
				cell.SetRenderer(renderer);
			}

			if (cell is IConstrainedCell constrainedCell)
			{
				_layout.PrepareCellForLayout(constrainedCell);
			}
		}

		IVisualElementRenderer CreateRenderer(View view)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));

			var renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}
	}
}