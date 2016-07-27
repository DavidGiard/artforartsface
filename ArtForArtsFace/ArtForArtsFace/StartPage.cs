using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System.IO;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;

namespace ArtForArtsFace
{
    public class StartPage : ContentPage
    {
        Image ArtworkImage;
        Image MyFaceImage;
        Label TitleLabel;
        Button AddMyFaceFromCameraButton = new Button();
        Button AddMyFaceFromSavedPhotoButton;
        int ArtFaceTop = 0;
        int ArtFaceLeft  = 0;
        int ArtFaceHeight = 0;
        int ArtFaceWidth = 0;
        ScrollView scrollView;

        private readonly IFaceServiceClient faceServiceClient = new FaceServiceClient("bf202105e10147fca8e21bc964d03894");

        public StartPage()
        {
            TitleLabel = new Label
            {
                Text = "Art for Art's Face",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
            };


            ArtworkImage = new Image
            {
                Source = "TheScream.png"
            };

            MyFaceImage = new Image
            {
                Source = "icon.png"
            };

            AddMyFaceFromCameraButton = new Button
            {
                Text = "Add My Face (Camera)!",
                Font = Font.SystemFontOfSize(NamedSize.Large),
                BorderWidth = 1,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            AddMyFaceFromCameraButton.Clicked += AddMyFaceFromCameraButton_Clicked;

            AddMyFaceFromSavedPhotoButton = new Button
            {
                Text = "Add My Face (Saved photo)!",
                Font = Font.SystemFontOfSize(NamedSize.Large),
                BorderWidth = 1,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            AddMyFaceFromSavedPhotoButton.Clicked += AddMyFaceFromSavedPhotoButton_Clicked;

            scrollView = new ScrollView();
            scrollView.Orientation = ScrollOrientation.Vertical;
            var stackLayout = new StackLayout();
            scrollView.Content = stackLayout;

            stackLayout.Children.Add(TitleLabel);
            stackLayout.Children.Add(ArtworkImage);
            stackLayout.Children.Add(AddMyFaceFromCameraButton);
            stackLayout.Children.Add(AddMyFaceFromSavedPhotoButton);
            stackLayout.Children.Add(MyFaceImage);


            Content = scrollView;
        }



        private async void AddMyFaceFromCameraButton_Clicked(object sender, EventArgs e)
        {
            Stream photoStream = await GetPhotoFromCamera();
            await ProcessPictureStream(photoStream);

        }

        private async void AddMyFaceFromSavedPhotoButton_Clicked(object sender, EventArgs e)
        {
            Stream photoStream = await GetPhotoFromGallery();
            await ProcessPictureStream(photoStream);
        }

        //private static Image CropImage(Image img, Rectangle cropArea)
        //{
        //    Bitmap bmpImage = new Bitmap(img);
        //    return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        //}

        private async Task<Stream> GetPhotoFromCamera()
        {

            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                throw new UnauthorizedAccessException("Camera is not available");
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "Sample",
                Name = "test.jpg"
            });

            Stream photoStream;
            photoStream = file.GetStream();
            file.Dispose();

            return photoStream;


        }

        private async Task<Stream> GetPhotoFromGallery()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                throw new UnauthorizedAccessException("Pick Photo is not supported");
            }


            MediaFile photoFile = await CrossMedia.Current.PickPhotoAsync();
            Stream photoStream = photoFile.GetStream();



            return photoStream;



        }

        private async Task ProcessPictureStream(Stream photoStream)
        {


            // TODO: Rotate face before sending to Cognnitive Svcs


            FaceRectangle[] faces = await UploadAndDetectFaces(photoStream);
            if (faces.Length <= 0)
            {
                await DisplayAlert("Warning", "No faces detetected in photo", "OK");
                return;
            }
            FaceRectangle firstFace = faces[0];
            int top = 0;
            int left = 0;
            int height = 0;
            int width = 0;

            top = firstFace.Top;
            left = firstFace.Left;
            height = firstFace.Height;
            width = firstFace.Width;


            MyFaceImage.Source = ImageSource.FromStream(() => photoStream);
            var msg = String.Format($"top: {top}; left: {left}; heigh: {height}; width: {width}");
            await DisplayAlert("title", msg, "OK");

            // TODO: Crop face and display on top of ArtImage
        }

        private async Task<FaceRectangle[]> UploadAndDetectFaces(Stream imageStream)
        {
            try
            {
                    var faces = await faceServiceClient.DetectAsync(imageStream);
                    var faceRects = faces.Select(face => face.FaceRectangle);
                    return faceRects.ToArray();
            }
            catch (Exception)
            {
                return new FaceRectangle[0];
            }
        }

    }
}
