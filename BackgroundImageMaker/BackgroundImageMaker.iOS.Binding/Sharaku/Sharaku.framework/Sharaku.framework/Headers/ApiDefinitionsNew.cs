using System;
using Foundation;
using UIKit;

// @interface SHViewController : UIViewController
[BaseType (typeof(UIViewController), Name = "_TtC7Sharaku16SHViewController")]
interface SHViewController
{
	// -(instancetype _Nonnull)initWithImage:(UIImage * _Nonnull)image __attribute__((objc_designated_initializer));
	[Export ("initWithImage:")]
	[DesignatedInitializer]
	IntPtr Constructor (UIImage image);

	// -(instancetype _Nullable)initWithCoder:(NSCoder * _Nonnull)aDecoder __attribute__((objc_designated_initializer));
	[Export ("initWithCoder:")]
	[DesignatedInitializer]
	IntPtr Constructor (NSCoder aDecoder);

	// -(void)loadView;
	[Export ("loadView")]
	void LoadView ();

	// -(void)viewDidLoad;
	[Export ("viewDidLoad")]
	void ViewDidLoad ();
}

// @interface Sharaku_Swift_157 (SHViewController) <UICollectionViewDataSource, UICollectionViewDelegate>
[Category]
[BaseType (typeof(SHViewController))]
interface SHViewController_Sharaku_Swift_157 : IUICollectionViewDataSource, IUICollectionViewDelegate
{
	// -(UICollectionViewCell * _Nonnull)collectionView:(UICollectionView * _Nonnull)collectionView cellForItemAtIndexPath:(NSIndexPath * _Nonnull)indexPath __attribute__((warn_unused_result));
	[Export ("collectionView:cellForItemAtIndexPath:")]
	UICollectionViewCell CollectionView (UICollectionView collectionView, NSIndexPath indexPath);

	// -(NSInteger)collectionView:(UICollectionView * _Nonnull)collectionView numberOfItemsInSection:(NSInteger)section __attribute__((warn_unused_result));
	[Export ("collectionView:numberOfItemsInSection:")]
	nint CollectionView (UICollectionView collectionView, nint section);

	// -(void)collectionView:(UICollectionView * _Nonnull)collectionView didSelectItemAtIndexPath:(NSIndexPath * _Nonnull)indexPath;
	[Export ("collectionView:didSelectItemAtIndexPath:")]
	void CollectionView (UICollectionView collectionView, NSIndexPath indexPath);
}
