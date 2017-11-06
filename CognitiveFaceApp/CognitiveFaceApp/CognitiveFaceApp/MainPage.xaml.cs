using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.ProjectOxford.Face;
using PCLStorage;
using Plugin.Media;
using Plugin.Media.Abstractions;

namespace CognitiveFaceApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void RunButton_OnClicked(object sender, EventArgs e)
        {
            // 画像の選択
            var photo = "";
            var imageChoiceResult = await DisplayAlert("どちらの画像を使いますか", "", "カメラ", "ローカルフォルダー");

            try
            {
                if (imageChoiceResult)
                {
                    photo = await TakePhotoAsync();
                }
                else
                {
                    photo = await PickPhotoAsync();
                }
            }
            catch (Exception exception)
            {
                await DisplayAlert("エラーが発生しました", exception.Message, "OK");
            }

            // 画像の判定
            ImagePreview.Source = photo;
            var faceResult = new FaceDetectResult();

            try
            {
                faceResult = await DetectFaceAsync(photo);
            }
            catch (Exception exception)
            {
                await DisplayAlert("Face API の呼び出しに失敗しました", exception.Message, "OK");
                return;
            }

            // 判定結果を配置
            Age.Text = "年齢 : " + faceResult.age;
            Gender.Text = "性別 : " + faceResult.gender;

            Emotion.Text = "表情 : ";
            switch (faceResult.emotionKey)
            {
                case "Anger":
                    Emotion.Text = Emotion.Text + "怒り";
                    break;
                case "Contempt":
                    Emotion.Text = Emotion.Text + "軽蔑";
                    break;
                case "Disgust":
                    Emotion.Text = Emotion.Text + "むかつき";
                    break;
                case "Fear":
                    Emotion.Text = Emotion.Text + "恐れ";
                    break;
                case "Happiness":
                    Emotion.Text = Emotion.Text + "喜び";
                    break;
                case "Neutral":
                    Emotion.Text = Emotion.Text + "無表情";
                    break;
                case "Sadness":
                    Emotion.Text = Emotion.Text + "悲しみ";
                    break;
                case "Surprise":
                    Emotion.Text = Emotion.Text + "驚き";
                    break;
                default:
                    break;
            }
            Emotion.Text = Emotion.Text + "(" + faceResult.emotionValue.ToString("0.00%") + ")";


        }
        private async void ResetButton_OnClicked(object sender, EventArgs e)
        {
            ImagePreview.Source = "";

            Age.Text = "年齢";
            Gender.Text = "性別";
            Emotion.Text = "表情";
        }


        public static async Task<string> TakePhotoAsync()
        {
            // カメラを初期化
            await CrossMedia.Current.Initialize();

            // カメラを使えるかどうか判定
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                throw new NotSupportedException("カメラをアプリから利用できるように設定してください");
            }

            // 撮影し、保存したファイルを取得
            var photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions());

            // 保存したファイルのパスを取得
            return photo.Path;
        }

        public static async Task<string> PickPhotoAsync()
        {
            // ローカルフォルダーから写真を選択させる
            var photo = await CrossMedia.Current.PickPhotoAsync();

            // 保存したファイルのパスを取得
            return photo.Path;

        }


        public class FaceDetectResult
        {
            public double age { get; set; }
            public string gender { get; set; }
            public string emotionKey { get; set; }
            public float emotionValue { get; set; }
        }

        public static async Task<FaceDetectResult> DetectFaceAsync(string photo)
        {
            // Face API 呼び出し準備
            var subKey = "Your_FaceAPISubKey";
            var client = new FaceServiceClient(subKey);

            // Face API で判定
            var file = await FileSystem.Current.GetFileFromPathAsync(photo);
            var imageStream = await file.OpenAsync(FileAccess.Read);
            var result = await client.DetectAsync(imageStream, false, false, Enum.GetValues(typeof(FaceAttributeType)).OfType<FaceAttributeType>().ToArray());

            // 判定結果を代入
            var detectResult = new FaceDetectResult();

            detectResult.age = result[0].FaceAttributes.Age;
            detectResult.gender = result[0].FaceAttributes.Gender;
            detectResult.emotionKey = result[0].FaceAttributes.Emotion.ToRankedList().First<KeyValuePair<string, float>>().Key;
            detectResult.emotionValue = result[0].FaceAttributes.Emotion.ToRankedList().First<KeyValuePair<string, float>>().Value;

            return detectResult;
        }


    }
}
