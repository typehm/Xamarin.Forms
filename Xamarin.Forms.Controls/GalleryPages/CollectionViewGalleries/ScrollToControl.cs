﻿using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ScrollToIndexControl : ContentView
	{
		readonly Entry _entry;
		readonly Switch _animateSwitch;
		readonly Label _currentIndex;
		ScrollToPosition _currentScrollToPosition;
		readonly CollectionView _cv;
		int _index;

		public ScrollToIndexControl(CollectionView cv)
		{
			_cv = cv;

			var indexLabel = new Label { Text = "Scroll To Index: ", VerticalTextAlignment = TextAlignment.Center};
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = "0", WidthRequest = 200 };
			var indexButton = new Button { Text = "Go" };

			indexButton.Clicked += ScrollTo;

			var row1 = new StackLayout { Orientation = StackOrientation.Horizontal };
			row1.Children.Add(indexLabel);
			row1.Children.Add(_entry);
			row1.Children.Add(indexButton);

			var animateLabel = new Label { Text = "Animate: ", VerticalTextAlignment = TextAlignment.Center};
			_animateSwitch = new Switch { IsToggled = true };

			var row2 = new StackLayout { Orientation = StackOrientation.Horizontal };
			row2.Children.Add(animateLabel);
			row2.Children.Add(_animateSwitch);

			_currentIndex = new Label
			{
				Text = _index.ToString(),
				WidthRequest = 100,
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center
			};

			var forwardButton = new Button { Text = "Advance 1 >>"};
			forwardButton.Clicked += (sender, args) =>
			{
				_index = _index + 1;
				_currentIndex.Text = _index.ToString();
				ScrollToIndex(_index);
			};
			
			var backButton = new Button { Text = "<< Back 1"};
			backButton.Clicked += (sender, args) =>
			{
				if (_index > 0)
				{
					_index = _index - 1;
					_currentIndex.Text = _index.ToString();
					ScrollToIndex(_index);
				}
			};

			var row3 = new StackLayout { Orientation = StackOrientation.Horizontal };
			row3.Children.Add(backButton);
			row3.Children.Add(_currentIndex);
			row3.Children.Add(forwardButton);

			var row4 = new StackLayout { Orientation = StackOrientation.Horizontal };

			var scrollPositionSelector = new EnumSelector<ScrollToPosition>(() => _currentScrollToPosition,
				type => _currentScrollToPosition = type);
			row4.Children.Add(scrollPositionSelector);

			var layout = new StackLayout { Margin = 3 };
			layout.Children.Add(row1);
			layout.Children.Add(row2);
			layout.Children.Add(row3);
			layout.Children.Add(row4);

			Content = layout;

			IsClippedToBounds = true;
		}

		void ScrollToIndex(int index)
		{
			_cv.ScrollTo(index, position: _currentScrollToPosition, animate: _animateSwitch.IsToggled);
		}

		void ScrollTo()
		{
			if (int.TryParse(_entry.Text, out int index))
			{
				ScrollToIndex(index);
				_index = index;
				_currentIndex.Text = _index.ToString();
			}
		}

		void ScrollTo(object sender, EventArgs e)
		{
			ScrollTo();
		}
	}
}