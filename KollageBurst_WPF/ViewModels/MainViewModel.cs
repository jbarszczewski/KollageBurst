using GalaSoft.MvvmLight;
using KollageBurst_WPF.Models;
using System.IO;
using System.Windows;
using GalaSoft.MvvmLight;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System;
using System.Windows.Media;
using KollageBurstLib;
using System.Windows.Controls;

namespace KollageBurst_WPF.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private WriteableBitmap processedBitmap;

        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }

            set
            {
                if (_welcomeTitle == value)
                {
                    return;
                }

                _welcomeTitle = value;
                RaisePropertyChanged(WelcomeTitlePropertyName);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

        #region Private methods
        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();
            if (result.Value)
            {
                WriteableBitmap writeableBitmap = ProcessBitmap(openFileDialog.FileName);
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Files | *.png";
            saveFileDialog.DefaultExt = "png";
            saveFileDialog.AddExtension = true;
            bool? result = saveFileDialog.ShowDialog();
            if (result.Value)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Interlace = PngInterlaceOption.Off;
                encoder.Frames.Add(BitmapFrame.Create(processedBitmap));
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    encoder.Save(fs);
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        /// <remarks> correct commented out code in mvvm aproach</remarks>
        private WriteableBitmap ProcessBitmap(string imagePath)
        {
            var originalImage = new BitmapImage(new Uri(imagePath));
            processedBitmap = new WriteableBitmap(originalImage.PixelWidth, originalImage.PixelHeight, 96, 96, PixelFormats.Pbgra32, null);
            WriteableBitmap originalWriteableBitmap = BitmapFactory.ConvertToPbgra32Format(originalImage);

            //this.OriginalImage.Source = processedBitmap;
            //this.ModifiedImage.Source = originalImage;

            int horizontalResolution = 50;
            int verticalResolution = 50;
            int panelWidth = (int)(originalWriteableBitmap.PixelWidth / horizontalResolution);
            int panelHeight = (int)(originalWriteableBitmap.PixelHeight / verticalResolution);

            //this.MosaicGrid.Columns = verticalResolution;
            //this.MosaicGrid.Rows = horizontalResolution;
            for (int i = 0; i < verticalResolution; i++)
            {
                for (int j = 0; j < horizontalResolution; j++)
                {
                    int top = panelHeight * i;
                    int left = panelWidth * j;
                    Color averageColor = originalWriteableBitmap.GetAverageColor(top, left, panelHeight, panelWidth);
                    Border newBorder = new Border
                    {
                        BorderThickness = new Thickness(1, 1, 0, 0),
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        Background = new SolidColorBrush(averageColor)
                    };
                    //this.MosaicGrid.Children.Add(newBorder);

                    processedBitmap.Blit(new Rect(left, top, panelWidth, panelHeight), originalWriteableBitmap, new Rect(0, 0, originalWriteableBitmap.PixelWidth, originalWriteableBitmap.PixelHeight), averageColor, WriteableBitmapExtensions.BlendMode.None);
                }
            }

            return processedBitmap;
        }

        #endregion
    }
}