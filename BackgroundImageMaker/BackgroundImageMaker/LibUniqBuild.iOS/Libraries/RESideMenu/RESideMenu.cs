using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Foundation;
using UIKit;
using System.Drawing;
using CoreGraphics;
using System.ComponentModel;

namespace LibUniqBuild.iOS.Libraries.RESideMenu
{
    [Register("RESideMenu"), DesignTimeVisible(true)]
    public class RESideMenu : UIViewController
    {
        private UIImage _backgroundImage;
        private UIViewController _contentViewController;
        private UIViewController _leftMenuViewController;
        private UIViewController _rightMenuViewController;
        private UIImageView _backgroundImageView;
        private UIButton _contentButton;
        private bool _visible;
        private CGPoint _originalPoint;

        private UIView _menuViewContainer;
        private UIView _contentViewContainer;
        private bool _didNotifyDelegate;

        private MenuState _menuState = MenuState.None;
        enum MenuState
        {
            None,
            LeftOpening,
            RightOpening,
            LeftClosing,
            RightClosing,
            LeftOpened,
            RightOpened,
        }

        public UIView ContentViewContainer { get { return _contentViewContainer; } }

        [Export("ContentViewStoryboardID"), Browsable(true)]
        public string ContentViewStoryboardID { get; set; }
        [Export("LeftMenuViewStoryboardID"), Browsable(true)]
        public string LeftMenuViewStoryboardID { get; set; }
        [Export("RightMenuViewStoryboardID"), Browsable(true)]
        public string RightMenuViewStoryboardID { get; set; }


        public event EventHandler<UIViewController> WillShowMenuViewController;
        public event EventHandler<UIViewController> DidShowMenuViewController;
        public event EventHandler<UIViewController> WillHideMenuViewController;
        public event EventHandler<UIViewController> DidHideMenuViewController;
        public event EventHandler<UIPanGestureRecognizer> DidRecognizePanGesture;


        public bool LeftMenuVisible { get; set; }
        public bool RightMenuVisible { get; set; }

        public float AnimationDuration { get; set; }
        public bool PanGestureEnabled { get; set; }
        public bool PanFromEdge { get; set; }
        public nuint PanMinimumOpenThreshold { get; set; }
        public bool InteractivePopGestureRecognizerEnabled { get; set; }
        public bool FadeMenuView { get; set; }
        public bool ScaleContentView { get; set; }
        public bool ScaleBackgroundImageView { get; set; }
        public bool ScaleMenuView { get; set; }
        public bool ContentViewShadowEnabled { get; set; }

        public UIColor ContentViewShadowColor { get; set; }
        public CGSize ContentViewShadowOffset { get; set; }
        public float ContentViewShadowOpacity { get; set; }
        public float ContentViewShadowRadius { get; set; }
        public float ContentViewFadeOutAlpha { get; set; }
        public float ContentViewScaleValue { get; set; }
        public float ContentViewInLandscapeOffsetCenterX { get; set; }
        public float ContentViewInPortraitOffsetCenterX { get; set; }

        public float ParallaxMenuMinimumRelativeValue { get; set; }
        public float ParallaxMenuMaximumRelativeValue { get; set; }
        public float ParallaxContentMinimumRelativeValue { get; set; }
        public float ParallaxContentMaximumRelativeValue { get; set; }

        public CGAffineTransform MenuViewControllerTransformation { get; set; }

        public bool ParallaxEnabled { get; set; }
        public bool BouncesHorizontally { get; set; }

        public UIStatusBarStyle MenuPreferredStatusBarStyle { get; set; }
        public bool MenuPrefersStatusBarHidden { get; set; }

        public UIImage BackgroundImage
        {
            get { return _backgroundImage; }
            set
            {
                _backgroundImage = value;
                if (_backgroundImageView != null)
                    _backgroundImageView.Image = _backgroundImage;
            }
        }

        public UIViewController ContentViewController
        {
            get { return _contentViewController; }
            set
            {
                if (_contentViewController == null)
                {
                    _contentViewController = value;
                    return;
                }
                HideViewController(_contentViewController);
                _contentViewController = value;

                AddChildViewController(_contentViewController);
                var rect = View.Bounds;
                if (bannerView != null) rect.Height -= (nfloat)bannerView.Frame.Height;
                _contentViewController.View.Frame = rect;
                _contentViewContainer.AddSubview(_contentViewController.View);
                _contentViewController.DidMoveToParentViewController(this);

                UpdateContentViewShadow();
    
                if (_visible) AddContentViewControllerMotionEffects();
            }
        }

        public UIViewController LeftMenuViewController {
            get
            {
                return _leftMenuViewController;
            }
            set
            {
                if (_leftMenuViewController == null)
                {
                    _leftMenuViewController = value;
                    return;
                }
                HideViewController(_leftMenuViewController);
                _leftMenuViewController = value;

                AddChildViewController(_leftMenuViewController);
                _leftMenuViewController.View.Frame = View.Bounds;
                _leftMenuViewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _menuViewContainer.AddSubview(_leftMenuViewController.View);
                _leftMenuViewController.DidMoveToParentViewController(this);

                AddMenuViewControllerMotionEffects();
                View.BringSubviewToFront(_contentViewContainer);
            }
        }
        public UIViewController RightMenuViewController {
            get { return _rightMenuViewController;  }
            set
            {
                if (_rightMenuViewController == null)
                {
                    _rightMenuViewController = value;
                    return;
                }
                HideViewController(_rightMenuViewController);
                _rightMenuViewController = value;

                AddChildViewController(_rightMenuViewController);
                _rightMenuViewController.View.Frame = View.Bounds;
                _rightMenuViewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _menuViewContainer.AddSubview(_rightMenuViewController.View);
                _rightMenuViewController.DidMoveToParentViewController(this);

                AddMenuViewControllerMotionEffects();
                View.BringSubviewToFront(_contentViewContainer);
            }
        }

        private UIView bannerView;
        private CGSize bannerSize;

        public RESideMenu()
        {
            CommonInit();
        }

        public RESideMenu(NSCoder coder) :
            base(coder)
        {
            CommonInit();
        }

        public RESideMenu(UIViewController contentViewController, UIViewController leftMenuViewController, UIViewController rightMenuViewController) :
            this()
        {
            ContentViewController = contentViewController;
            LeftMenuViewController = leftMenuViewController;
            RightMenuViewController = rightMenuViewController;
        }

        public override void AwakeFromNib()
        {
            if (!string.IsNullOrEmpty(ContentViewStoryboardID))
            {
                ContentViewController = Storyboard.InstantiateViewController(ContentViewStoryboardID);
            }

            if (!string.IsNullOrEmpty(LeftMenuViewStoryboardID))
            {
                LeftMenuViewController = Storyboard.InstantiateViewController(LeftMenuViewStoryboardID);
            }
            if (!string.IsNullOrEmpty(RightMenuViewStoryboardID))
            {
                RightMenuViewController = Storyboard.InstantiateViewController(RightMenuViewStoryboardID);
            }
        }

        private void CommonInit()
        {
            _menuViewContainer = new UIView();
            _contentViewContainer = new UIView();
            AnimationDuration = 0.35f;
            InteractivePopGestureRecognizerEnabled = true;
            MenuViewControllerTransformation = CGAffineTransform.MakeScale(1.5F, 1.5F);

            ScaleContentView = true;
            ScaleBackgroundImageView = true;
            ScaleMenuView = true;
            FadeMenuView = true;

            ParallaxEnabled = true;
            ParallaxMenuMinimumRelativeValue = -15;
            ParallaxMenuMaximumRelativeValue = 15;
            ParallaxContentMinimumRelativeValue = -25;
            ParallaxContentMaximumRelativeValue = 25;

            BouncesHorizontally = true;

            PanGestureEnabled = true;
            PanFromEdge = true;
            PanMinimumOpenThreshold = 60;

            ContentViewShadowEnabled = false;
            ContentViewShadowColor = UIColor.Black;
            ContentViewShadowOffset = CGSize.Empty;
            ContentViewShadowOpacity = 0.4f;
            ContentViewShadowRadius = 8.0f;
            ContentViewFadeOutAlpha = 1.0f;
            ContentViewInLandscapeOffsetCenterX = 30;
            ContentViewInPortraitOffsetCenterX = 30;
            ContentViewScaleValue = 0.7f;
        }

        public void PresentLeftMenuViewController()
        {
            PresentMenuViewContainerWithMenuViewController(LeftMenuViewController);
            ShowLeftMenuViewController();
        }

        public void PresentRightMenuViewController()
        {
            PresentMenuViewContainerWithMenuViewController(RightMenuViewController);
            ShowRightMenuViewController();
        }

        public void HideMenuViewController()
        {
            HideMenuViewControllerAnimated(true);
        }

        public void SetContentViewController(UIViewController contentViewController, bool animated)
        {
            if (_contentViewController == contentViewController)
            {
                return;
            }

            if (!animated)
            {
                ContentViewController = contentViewController;
            }
            else
            {
                try
                {
                    AddChildViewController(contentViewController);
                    contentViewController.View.Alpha = 0;
                    var rect = _contentViewContainer.Bounds;
                    if (bannerView != null) rect.Height -= (nfloat)bannerView.Frame.Height;
                    contentViewController.View.Frame = rect;
                    _contentViewContainer.AddSubview(contentViewController.View);
                    UIView.Animate(AnimationDuration, () => {
                        contentViewController.View.Alpha = 1;
                    },
                    () => {
                    //contentViewController.View.RemoveFromSuperview();
                    //ContentViewController = contentViewController;
                        HideViewController(ContentViewController);
                        contentViewController.DidMoveToParentViewController(this);
                        _contentViewController = contentViewController;
                        StatusBarNeedsAppearanceUpdate();
                        UpdateContentViewShadow();
                        if (_visible)
                        {
                            AddContentViewControllerMotionEffects();
                        }
                    });
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("e:" + e.Message);
                }
                
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (ContentViewInLandscapeOffsetCenterX == 0.0f)
                ContentViewInLandscapeOffsetCenterX = (float)View.Frame.Height + 30f;

            if (ContentViewInPortraitOffsetCenterX == 0.0f)
                ContentViewInPortraitOffsetCenterX = (float)View.Frame.Width + 30f;

            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _backgroundImageView = new UIImageView(View.Bounds)
            {
                Image = _backgroundImage,
                ContentMode = UIViewContentMode.ScaleAspectFill,
                AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
            };
            _contentButton = UIButton.FromType(UIButtonType.Custom);
            _contentButton.TouchUpInside += (sender, e) => {
                HideMenuViewController();
            };


            View.AddSubview(_backgroundImageView);
            View.AddSubview(_menuViewContainer);
            View.AddSubview(_contentViewContainer);

            _menuViewContainer.Frame = View.Bounds;
            _menuViewContainer.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;


            if (LeftMenuViewController != null)
            {
                AddChildViewController(LeftMenuViewController);
                LeftMenuViewController.View.Frame = View.Bounds;
                LeftMenuViewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _menuViewContainer.AddSubview(LeftMenuViewController.View);
                LeftMenuViewController.DidMoveToParentViewController(this);
            }

            if (RightMenuViewController != null)
            {
                AddChildViewController(RightMenuViewController);
                RightMenuViewController.View.Frame = View.Bounds;
                RightMenuViewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                _menuViewContainer.AddSubview(RightMenuViewController.View);
                RightMenuViewController.DidMoveToParentViewController(this);
            }

            _contentViewContainer.Frame = View.Bounds;
            _contentViewContainer.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            AddChildViewController(ContentViewController);
            var rect = View.Bounds;
            if (bannerView != null) rect.Height -= (nfloat)bannerView.Frame.Height;
            ContentViewController.View.Frame = rect;
            _contentViewContainer.AddSubview(ContentViewController.View);
            ContentViewController.DidMoveToParentViewController(this);

            _menuViewContainer.Alpha = 0;

            if (ScaleBackgroundImageView)
                _backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);

            AddMenuViewControllerMotionEffects();

            if (PanGestureEnabled)
            {
                View.MultipleTouchEnabled = false;
                var gesture = new UIPanGestureRecognizer(PanGestureRecognized);
                gesture.ShouldReceiveTouch += (r, t) =>
                {
                    return ShouldReceiveTouch(r, t);
                };
                View.AddGestureRecognizer(gesture);
            }
        }

        public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
        {
            coordinator.AnimateAlongsideTransition((context) =>
            {
                var orientation = UIApplication.SharedApplication.StatusBarOrientation;
                var y = toSize.Height - bannerSize.Height;
                var width = toSize.Width;
                bannerView.Frame = new CGRect((width - bannerSize.Width) / 2.0, y, bannerSize.Width, bannerSize.Height);
            }, 
            (context) =>
            {
            });
            base.ViewWillTransitionToSize(toSize, coordinator);
        }

        public void AddBannerView(UIView bannerView, CGSize bannerSize)
        {
            this.bannerView = bannerView;
            this.bannerSize = bannerSize;
            bannerView.Frame = new CGRect(bannerView.X(), _contentViewContainer.Frame.Height - bannerSize.Height, bannerSize.Width, bannerSize.Height);
            var rect = _contentViewContainer.Bounds;
            if (bannerView != null) rect.Height -= (nfloat)bannerSize.Height;
            ContentViewController.View.Frame = rect;
            _contentViewContainer.AddSubview(bannerView);
            _contentViewContainer.BackgroundColor = UIColor.White;
        }

        private void PresentMenuViewContainerWithMenuViewController(UIViewController menuViewController)
        {
            _menuViewContainer.Transform = CGAffineTransform.MakeIdentity();
            if (ScaleBackgroundImageView)
            {
                _backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
                _backgroundImageView.Frame = View.Bounds;
            }
            _menuViewContainer.Frame = View.Bounds;
            if (ScaleMenuView)
            {
                _menuViewContainer.Transform = MenuViewControllerTransformation;
            }
            _menuViewContainer.Alpha = !FadeMenuView ? 1 : 0;

            if (ScaleBackgroundImageView)
            {
                _backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);
            }

            WillShowMenuViewController?.Invoke(this, menuViewController);
        }

        public void ShowLeftMenuViewController()
        {
            if (LeftMenuViewController == null) {
                return;
            }
            LeftMenuViewController.BeginAppearanceTransition(true, true);
            LeftMenuViewController.View.Hidden = false;
            if (RightMenuViewController != null)
            {
                RightMenuViewController.View.Hidden = true;
            }
            View.Window.EndEditing(true);
            AddContentButton();
            UpdateContentViewShadow();
            ResetContentViewScale();

            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
            UIView.Animate(AnimationDuration, () =>
            {
                if (ScaleContentView)
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeScale(ContentViewScaleValue, ContentViewScaleValue);
                }
                else
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                }
                var currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
                bool isPortrait = currentOrientation == UIInterfaceOrientation.Portrait
                    || currentOrientation == UIInterfaceOrientation.PortraitUpsideDown;

                var centerPointX = isPortrait ? ContentViewInPortraitOffsetCenterX + View.Frame.Width : ContentViewInLandscapeOffsetCenterX + View.Frame.Width;
                var centerPointY = _contentViewContainer.Center.Y;
                _contentViewContainer.Center = new CGPoint(centerPointX, centerPointY);

                _menuViewContainer.Alpha = !FadeMenuView ? 0 : 1.0f;
                _contentViewContainer.Alpha = ContentViewFadeOutAlpha;
                _menuViewContainer.Transform = CGAffineTransform.MakeIdentity();
                if (ScaleBackgroundImageView)
                    _backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
            }, 
            () => {
                LeftMenuViewController.EndAppearanceTransition();
                if (!_visible)
                {
                    DidShowMenuViewController?.Invoke(this, LeftMenuViewController);
                }
                _visible = true;
                LeftMenuVisible = _visible;
                _menuState = MenuState.LeftOpened;
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                AddContentViewControllerMotionEffects();
            });
            StatusBarNeedsAppearanceUpdate();
        }

        private void ShowRightMenuViewController()
        {
            if (RightMenuViewController == null) {
                return;
            }
            RightMenuViewController.BeginAppearanceTransition(true, true);
            RightMenuViewController.View.Hidden = false;
            if (LeftMenuViewController != null)
            {
                LeftMenuViewController.View.Hidden = true;
            }
            View.Window.EndEditing(true);
            AddContentButton();

            UpdateContentViewShadow();
            ResetContentViewScale();


            UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
            UIView.Animate(AnimationDuration, () =>
            {
                if (ScaleContentView)
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeScale(ContentViewScaleValue, ContentViewScaleValue);
                }
                else
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                }

                var currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
                bool isPortrait = currentOrientation == UIInterfaceOrientation.Portrait
                    || currentOrientation == UIInterfaceOrientation.PortraitUpsideDown;

                var centerPointX = isPortrait ? -ContentViewInPortraitOffsetCenterX : -ContentViewInLandscapeOffsetCenterX;
                var centerPointY = _contentViewContainer.Center.Y;
                _contentViewContainer.Center = new CGPoint(centerPointX, centerPointY);


                _menuViewContainer.Alpha = !FadeMenuView ? 0 : 1.0f;
                _contentViewContainer.Alpha = ContentViewFadeOutAlpha;
                _menuViewContainer.Transform = CGAffineTransform.MakeIdentity();
                if (ScaleBackgroundImageView)
                    _backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
            }, 
            () =>
            {
                RightMenuViewController.EndAppearanceTransition();
                if (!_visible)
                {
                    DidShowMenuViewController?.Invoke(this, RightMenuViewController);
                }
                _visible = !(_contentViewContainer.Frame.Width == View.Bounds.Width && _contentViewContainer.Frame.Height == View.Bounds.Height && _contentViewContainer.Frame.X == 0 && _contentViewContainer.Frame.Y == 0);
                RightMenuVisible = _visible;
                _menuState = MenuState.RightOpened;
                UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                AddContentViewControllerMotionEffects();
            });
            StatusBarNeedsAppearanceUpdate();
        }

        private void HideViewController(UIViewController viewController)
        {
            viewController.WillMoveToParentViewController(null);
            viewController.View.RemoveFromSuperview();
            viewController.RemoveFromParentViewController();
        }

        private void AnimateBlock()
        {
            _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
            _contentViewContainer.Frame = View.Bounds;
            if (ScaleMenuView)
            {
                _menuViewContainer.Transform = MenuViewControllerTransformation;
            }

            _menuViewContainer.Alpha = !FadeMenuView ? 1 : 0;
            _contentViewContainer.Alpha = 1;

            if (ScaleBackgroundImageView)
            {
                _backgroundImageView.Transform = CGAffineTransform.MakeScale(1.7f, 1.7f);
            }

            if (ParallaxEnabled)
            {
                foreach (UIMotionEffect effect in _contentViewContainer.MotionEffects)
                {
                    _contentViewContainer.RemoveMotionEffect(effect);
                }
            }
        }

        private void CompleteBlock(UIViewController visibleMenuViewController)
        {
            if (visibleMenuViewController != null)
            {
                visibleMenuViewController.EndAppearanceTransition();
                if (!_visible)
                {
                    DidHideMenuViewController?.Invoke(this, visibleMenuViewController);
                }
            }

            LeftMenuVisible = RightMenuVisible = _visible = false;
            _menuState = MenuState.None;
        }

        public void HideMenuViewControllerAnimated(bool animated)
        {
            var leftMenuVisible = _menuState == MenuState.LeftOpened || _menuState == MenuState.LeftOpening || _menuState == MenuState.LeftClosing;
            var rightMenuVisible = _menuState == MenuState.RightOpened || _menuState == MenuState.RightOpening || _menuState == MenuState.RightClosing;
            UIViewController visibleMenuViewController = null;
            if (leftMenuVisible) visibleMenuViewController = LeftMenuViewController;
            else if (rightMenuVisible) visibleMenuViewController = RightMenuViewController;

            if (visibleMenuViewController != null)
            {
                visibleMenuViewController.BeginAppearanceTransition(false, animated);
                WillHideMenuViewController?.Invoke(this, visibleMenuViewController);
            }
            _contentButton.RemoveFromSuperview();

            if (animated)
            {
                UIApplication.SharedApplication.BeginIgnoringInteractionEvents();
                UIView.Animate(AnimationDuration, () =>
                {
                    AnimateBlock();
                },
                () =>
                {
                    UIApplication.SharedApplication.EndIgnoringInteractionEvents();
                    CompleteBlock(visibleMenuViewController);
                });
            }
            else
            {
                AnimateBlock();
                CompleteBlock(visibleMenuViewController);
            }
            StatusBarNeedsAppearanceUpdate();
        }

        private void AddContentButton()
        {
            if (_contentButton.Superview != null)
                return;
            _contentButton.AutoresizingMask = UIViewAutoresizing.None;
            _contentButton.Frame = _contentViewController.View.Bounds;
            _contentButton.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            _contentViewController.View.AddSubview(_contentButton);
        }

        private void StatusBarNeedsAppearanceUpdate()
        {
            UIView.Animate(0.3, () =>
            {
                SetNeedsStatusBarAppearanceUpdate();
            });
        }

        private void UpdateContentViewShadow()
        {
            if (ContentViewShadowEnabled)
            {
                var layer = _contentViewContainer.Layer;
                var path = UIBezierPath.FromRect(layer.Bounds);
                layer.ShadowPath = path.CGPath;
                layer.ShadowColor = ContentViewShadowColor.CGColor;
                layer.ShadowOffset = ContentViewShadowOffset;
                layer.ShadowOpacity = ContentViewShadowOpacity;
                layer.ShadowRadius = ContentViewShadowRadius;
            }
        }

        private void ResetContentViewScale()
        {
            var t = _contentViewContainer.Transform;
            var scale = Math.Sqrt(t.xx * t.xx + t.xy * t.xy);
            CGRect Frame = _contentViewContainer.Frame;
            _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
            _contentViewContainer.Transform = CGAffineTransform.MakeScale((nfloat)scale, (nfloat)scale);
            _contentViewContainer.Frame = Frame;
        }

        private void AddMenuViewControllerMotionEffects()
        {
            if (ParallaxEnabled)
            {
                foreach (var me in _menuViewContainer.MotionEffects)
                {
                    _menuViewContainer.RemoveMotionEffect(me);
                }
                var interpolationHorizontal = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);
                interpolationHorizontal.MinimumRelativeValue = new NSNumber(ParallaxMenuMinimumRelativeValue);
                interpolationHorizontal.MaximumRelativeValue = new NSNumber(ParallaxMenuMaximumRelativeValue);

                var interpolationVertical = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);
                interpolationVertical.MinimumRelativeValue = new NSNumber(ParallaxMenuMinimumRelativeValue);
                interpolationVertical.MaximumRelativeValue = new NSNumber(ParallaxMenuMaximumRelativeValue);

                _menuViewContainer.AddMotionEffect(interpolationHorizontal);
                _menuViewContainer.AddMotionEffect(interpolationVertical);
            }
        }

        private void AddContentViewControllerMotionEffects()
        {
            if (ParallaxEnabled)
            {
                foreach (var me in _contentViewContainer.MotionEffects)
                {
                    _contentViewContainer.RemoveMotionEffect(me);
                }
                UIView.Animate(0.2, () => {
                    var interpolationHorizontal = new UIInterpolatingMotionEffect("center.x", UIInterpolatingMotionEffectType.TiltAlongHorizontalAxis);
                    interpolationHorizontal.MinimumRelativeValue = new NSNumber(ParallaxContentMinimumRelativeValue);
                    interpolationHorizontal.MaximumRelativeValue = new NSNumber(ParallaxContentMaximumRelativeValue);

                    var interpolationVertical = new UIInterpolatingMotionEffect("center.y", UIInterpolatingMotionEffectType.TiltAlongVerticalAxis);
                    interpolationVertical.MinimumRelativeValue = new NSNumber(ParallaxContentMinimumRelativeValue);
                    interpolationVertical.MaximumRelativeValue = new NSNumber(ParallaxContentMaximumRelativeValue);

                    _contentViewContainer.AddMotionEffect(interpolationHorizontal);
                    _contentViewContainer.AddMotionEffect(interpolationVertical);
                });
            }
        }

        // Gesture 

        private bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
        {
            if (InteractivePopGestureRecognizerEnabled && ContentViewController is UINavigationController) {
                var navigationController = ContentViewController as UINavigationController;
                if (navigationController.ViewControllers.Length > 1 && navigationController.InteractivePopGestureRecognizer.Enabled) {
                    return false;
                }
            }

            if (PanFromEdge && recognizer is UIPanGestureRecognizer && !_visible) {
                CGPoint point = touch.LocationInView(recognizer.View);
                if (point.X < 20.0 || point.X > View.Frame.Width - 20.0)
                {
                    return true;
                } else {
                    return false;
                }
            }
            return true;
        }

        private void PanGestureRecognized(UIPanGestureRecognizer recognizer)
        {
            DidRecognizePanGesture?.Invoke(this, recognizer);

            if (!PanGestureEnabled)
            {
                return;
            }
            try
            {
                var point = recognizer.TranslationInView(View);

                if (recognizer.State == UIGestureRecognizerState.Began)
                {
                    UpdateContentViewShadow();
                    _originalPoint = new CGPoint(_contentViewContainer.Center.X - _contentViewContainer.Bounds.Width / 2.0,
                                             _contentViewContainer.Center.Y - _contentViewContainer.Bounds.Height / 2.0);
                    _menuViewContainer.Transform = CGAffineTransform.MakeIdentity();
                    if (ScaleBackgroundImageView)
                    {
                        _backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
                        _backgroundImageView.Frame = View.Bounds;
                    }
                    _menuViewContainer.Frame = View.Bounds;
                    AddContentButton();
                    View.Window.EndEditing(true);
                    _didNotifyDelegate = false;
                }

                if (recognizer.State == UIGestureRecognizerState.Changed)
                {
                    nfloat delta = 0;
                    if (_visible)
                    {
                        delta = _originalPoint.X != 0 ? (point.X + _originalPoint.X) / _originalPoint.X : 0;
                    }
                    else
                    {
                        delta = point.X / View.Frame.Width;
                    }
                    delta = Math.Min(Math.Abs((float)delta), 1.6f);

                    var contentViewScale = ScaleContentView ? 1 - ((1 - ContentViewScaleValue) * delta) : 1;

                    var backgroundViewScale = 1.7f - (0.7f * delta);
                    var menuViewScale = 1.5f - (0.5f * delta);

                    if (!BouncesHorizontally)
                    {
                        contentViewScale = Math.Max((float)contentViewScale, ContentViewScaleValue);
                        backgroundViewScale = Math.Max((float)backgroundViewScale, 1.0f);
                        menuViewScale = Math.Max((float)menuViewScale, 1.0f);
                    }

                    _menuViewContainer.Alpha = !FadeMenuView ? 0 : delta;
                    _contentViewContainer.Alpha = 1 - (1 - ContentViewFadeOutAlpha) * delta;

                    if (ScaleBackgroundImageView)
                    {
                        _backgroundImageView.Transform = CGAffineTransform.MakeScale(backgroundViewScale, backgroundViewScale);
                    }

                    if (ScaleMenuView)
                    {
                        _menuViewContainer.Transform = CGAffineTransform.MakeScale(menuViewScale, menuViewScale);
                    }

                    if (ScaleBackgroundImageView)
                    {
                        if (backgroundViewScale < 1)
                        {
                            _backgroundImageView.Transform = CGAffineTransform.MakeIdentity();
                        }
                    }

                    if (!BouncesHorizontally && _visible)
                    {
                        if (_contentViewContainer.Frame.X > _contentViewContainer.Frame.Width / 2.0)
                            point.X = Math.Min(0.0f, (float)point.X);

                        if (_contentViewContainer.Frame.X < -(_contentViewContainer.Frame.Width / 2.0))
                            point.X = Math.Max(0.0f, (float)point.X);
                    }

                    // Limit size
                    //
                    if (point.X < 0)
                    {
                        point.X = Math.Max((float)point.X, (float)-UIScreen.MainScreen.Bounds.Height);
                    }
                    else
                    {
                        point.X = Math.Min((float)point.X, (float)UIScreen.MainScreen.Bounds.Height);
                    }
                    recognizer.SetTranslation(point, View);

                    if (!_didNotifyDelegate)
                    {
                        if (point.X > 0)
                        {
                            if (!_visible)
                            {
                                _menuState = MenuState.LeftOpening;
                                WillShowMenuViewController?.Invoke(this, LeftMenuViewController);
                            }
                            else
                            {
                                _menuState = MenuState.RightClosing;
                            }
                        }
                        if (point.X < 0)
                        {
                            if (!_visible)
                            {
                                _menuState = MenuState.RightOpening;
                                WillShowMenuViewController?.Invoke(this, RightMenuViewController);
                            }
                            else
                            {
                                _menuState = MenuState.LeftClosing;
                            }
                        }
                        _didNotifyDelegate = true;
                    }

                    if (contentViewScale > 1)
                    {
                        var oppositeScale = (1 - (contentViewScale - 1));
                        _contentViewContainer.Transform = CGAffineTransform.MakeScale(oppositeScale, oppositeScale);
                        _contentViewContainer.Transform = CGAffineTransform.Translate(_contentViewContainer.Transform, point.X, 0);
                    }
                    else
                    {
                        _contentViewContainer.Transform = CGAffineTransform.MakeScale(contentViewScale, contentViewScale);
                        _contentViewContainer.Transform = CGAffineTransform.Translate(_contentViewContainer.Transform, point.X, 0);
                    }

                    if (LeftMenuViewController != null)
                    {
                        LeftMenuViewController.View.Hidden = _contentViewContainer.Frame.X < 0;
                    }
                    if (RightMenuViewController != null)
                    {
                        RightMenuViewController.View.Hidden = _contentViewContainer.Frame.X > 0;
                    }

                    if (LeftMenuViewController == null && _contentViewContainer.Frame.X > 0)
                    {
                        _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                        _contentViewContainer.Frame = View.Bounds;
                        _visible = false;
                        LeftMenuVisible = false;
                    }
                    else if (RightMenuViewController == null && _contentViewContainer.Frame.X < 0)
                    {
                        _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                        _contentViewContainer.Frame = View.Bounds;
                        _visible = false;
                        RightMenuVisible = false;
                    }
                    StatusBarNeedsAppearanceUpdate();
                }
                if (recognizer.State == UIGestureRecognizerState.Ended)
                {
                    _didNotifyDelegate = false;
                    if (PanMinimumOpenThreshold > 0 && (
                        (_contentViewContainer.Frame.X < 0 && _contentViewContainer.Frame.X > -((int)PanMinimumOpenThreshold)) ||
                        (_contentViewContainer.Frame.X > 0 && _contentViewContainer.Frame.X < PanMinimumOpenThreshold))
                        )
                    {
                        HideMenuViewController();
                    }
                    else if (_contentViewContainer.Frame.X == 0)
                    {
                        HideMenuViewControllerAnimated(false);
                    }
                    else
                    {
                        if (recognizer.VelocityInView(View).X > 0)
                        {
                            if (_contentViewContainer.Frame.X < 0)
                            {
                                HideMenuViewController();
                            }
                            else
                            {
                                if (LeftMenuViewController != null)
                                {
                                    ShowLeftMenuViewController();
                                }
                            }
                        }
                        else
                        {
                            if (_contentViewContainer.Frame.X < 20)
                            {
                                if (RightMenuViewController != null)
                                {
                                    ShowRightMenuViewController();
                                }
                            }
                            else
                            {
                                HideMenuViewController();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception :" + e);
                System.Diagnostics.Debug.WriteLine("Stacktrace :" + e.StackTrace);
            }
        }

        // Setters
        public override bool ShouldAutorotate()
        {
            return ContentViewController.ShouldAutorotate();
        }

        public override void WillAnimateRotation(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            if (_visible)
            {
                _menuViewContainer.Bounds = View.Bounds;
                _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                _contentViewContainer.Frame = View.Bounds;

                if (ScaleContentView)
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeScale(ContentViewScaleValue, ContentViewScaleValue);
                }
                else
                {
                    _contentViewContainer.Transform = CGAffineTransform.MakeIdentity();
                }

                var landscape = UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeLeft
                                || UIApplication.SharedApplication.StatusBarOrientation == UIInterfaceOrientation.LandscapeRight;
                CGPoint center;
                if (LeftMenuVisible)
                {
                    var x = landscape ? ContentViewInLandscapeOffsetCenterX : ContentViewInPortraitOffsetCenterX;
                    var y = _contentViewController.View.Center.Y;
                    center = new CGPoint(x + View.Frame.Width, y);
                }
                else
                {
                    var x = landscape ? -ContentViewInLandscapeOffsetCenterX : -ContentViewInPortraitOffsetCenterX;
                    var y = _contentViewController.View.Center.Y;
                    center = new CGPoint(x, y);
                }
                _contentViewContainer.Center = center;
            }
        }

        private void UpdateStatusBar()
        {
            UIView.Animate(0.3f, () => {
                SetNeedsStatusBarAppearanceUpdate();
            });
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            var statusBarStyle = UIStatusBarStyle.Default;

            statusBarStyle = _visible ? MenuPreferredStatusBarStyle : ContentViewController.PreferredStatusBarStyle();
            if (_contentViewController.View.Frame.Y > 10)
            {
                statusBarStyle = MenuPreferredStatusBarStyle;
            }
            else
            {
                statusBarStyle = _contentViewController.PreferredStatusBarStyle();
            }

            return statusBarStyle;
        }

        public override bool PrefersStatusBarHidden()
        {
            var statusBarHidden = false;
            statusBarHidden = _visible ? MenuPrefersStatusBarHidden : _contentViewController.PrefersStatusBarHidden();
            if (_contentViewController.View.Frame.Y > 10)
            {
                statusBarHidden = MenuPrefersStatusBarHidden;
            }
            else
            {
                statusBarHidden = _contentViewController.PrefersStatusBarHidden();
            }
            return statusBarHidden;
        }

        public override UIStatusBarAnimation PreferredStatusBarUpdateAnimation
        {
            get
            {
                var statusBarAnimation = UIStatusBarAnimation.None;
                statusBarAnimation = _visible ? LeftMenuViewController.PreferredStatusBarUpdateAnimation : _contentViewController.PreferredStatusBarUpdateAnimation;
                if (_contentViewController.View.Frame.Y > 10)
                {
                    statusBarAnimation = LeftMenuViewController.PreferredStatusBarUpdateAnimation;
                }
                else
                {
                    statusBarAnimation = _contentViewController.PreferredStatusBarUpdateAnimation;
                }
                return statusBarAnimation;
            }
        }
    }

    public static class UIViewControllerExtensions
    {
        public static RESideMenu SideMenuViewController(this UIViewController viewController)
        {
            var iter = viewController.ParentViewController;
            while (iter != null)
            {
                if (iter is RESideMenu)
                {
                    return iter as RESideMenu;
                }
                else if (iter.ParentViewController != null && iter.ParentViewController != iter)
                {
                    iter = iter.ParentViewController;
                }
                else
                {
                    iter = null;
                }
            }
            return null;
        }

        public static void PresentLeftMenuViewController(this UIViewController viewController)
        {
            SideMenuViewController(viewController).PresentLeftMenuViewController();
        }

        public static void PresentRightMenuViewController(this UIViewController viewController)
        {
            SideMenuViewController(viewController).PresentRightMenuViewController();
        }

        public static void PresentLeftMenuViewController(this UIViewController viewController, object sender, EventArgs eventArgs)
        {
            SideMenuViewController(viewController).PresentLeftMenuViewController();
        }

        public static void PresentRightMenuViewController(this UIViewController viewController, object sender, EventArgs eventArgs)
        {
            SideMenuViewController(viewController).PresentRightMenuViewController();
        }

    }

}