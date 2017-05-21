using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using KollageBurst_WP8.Extensions;
using KollageBurst_WP8.Messages;
using KollageBurst_WP8.Models;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KollageBurst_WP8.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string SavePicturesPath = @"C:\Data\Users\Public\Pictures\Saved Pictures\";
        private string modifiedFileName = string.Empty;
        private IList<PhotoMetadata> photoDictionary;

        // Our settings
        private IsolatedStorageSettings settings;

        #region Options public properties

        /// <summary>
        /// Option to set type of source for image fillers
        /// </summary>
        public bool UseOriginalPhoto
        {
            get
            {
                return GetValueOrDefault<bool>("UseOriginalPhoto", true);
            }
            set
            {
                if (AddOrUpdateValue("UseOriginalPhoto", value))
                {
                    this.RaisePropertyChanged(() => this.UseOriginalPhoto);
                }
            }
        }

        /// <summary>
        /// Option to set max number of photo to use
        /// </summary>
        public int MaximumPhotoQuantity
        {
            get
            {
                return GetValueOrDefault<int>("MaximumPhotoQuantity", 100);
            }
            set
            {
                if (AddOrUpdateValue("MaximumPhotoQuantity", value))
                {
                    this.RaisePropertyChanged(() => this.MaximumPhotoQuantity);
                }
            }
        }

        /// <summary>
        /// Option to set horizontal size of grid
        /// </summary>
        public int HorizontalResolution
        {
            get
            {
                return GetValueOrDefault<int>("HorizontalResolution", 10);
            }
            set
            {
                if (AddOrUpdateValue("HorizontalResolution", value))
                {
                    this.RaisePropertyChanged(() => this.HorizontalResolution);
                }
            }
        }

        /// <summary>
        /// Option to set vertical size of grid
        /// </summary>
        public int VerticalResolution
        {
            get
            {
                return GetValueOrDefault<int>("VerticalResolution", 10);
            }
            set
            {
                if (AddOrUpdateValue("VerticalResolution", value))
                {
                    this.RaisePropertyChanged(() => this.VerticalResolution);
                }
            }
        }

        #endregion

        #region Main view public properties

        private bool isBusy;
        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                this.Set<bool>(() => this.IsBusy, ref this.isBusy, value);
            }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set
            {
                this.Set<string>(() => this.Message, ref this.message, value);
            }
        }

        private Photo originalPhoto;
        public Photo OriginalPhoto
        {
            get { return this.originalPhoto; }
            set
            {
                this.Set<Photo>(() => this.OriginalPhoto, ref this.originalPhoto, value);
            }
        }

        private Photo modifiedPhoto;
        public Photo ModifiedPhoto
        {
            get { return this.modifiedPhoto; }
            set
            {
                this.Set<Photo>(() => this.ModifiedPhoto, ref this.modifiedPhoto, value);
                this.SavePhotoCommand.RaiseCanExecuteChanged();
                this.SharePhotoCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand OpenPhotoCommand { get; private set; }
        public RelayCommand SavePhotoCommand { get; private set; }
        public RelayCommand ShowSettingsCommand { get; private set; }
        public RelayCommand SharePhotoCommand { get; private set; }
        public RelayCommand ShowAboutPageCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            // Get the settings for this application.
            this.settings = IsolatedStorageSettings.ApplicationSettings;

            var currentVersion = GoogleAnalytics.EasyTracker.GetTracker().AppVersion;
            if (this.settings.Contains("AppVersion"))
            {
                if (!this.settings["AppVersion"].Equals(currentVersion))
                {
                    GoogleAnalytics.EasyTracker.GetTracker().SendEvent("App updated", this.settings["AppVersion"] + " updated to " + currentVersion, string.Empty, 0);
                    this.settings["AppVersion"] = currentVersion;
                }
            }
            else
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("New install", "App installed. Version " + currentVersion, string.Empty, 0);
                this.settings["AppVersion"] = currentVersion;
            }

            this.PopulateDatabase();
            this.InitialiseCommands();
        }

        private void PopulateDatabase()
        {
            using (var photoColorDB = new PhotoColorContext())
            {
                if (!photoColorDB.DatabaseExists())
                {
                    photoColorDB.CreateDatabase();
                }

                if (photoColorDB.PhotoColors.Count() != this.MaximumPhotoQuantity)
                {
                    // build photo palette dictionary
                    MediaLibrary library = new MediaLibrary();
                    IEnumerable<Picture> photos = library.Pictures.AsEnumerable().Take(this.MaximumPhotoQuantity);
                    this.photoDictionary = new List<PhotoMetadata>();

                    foreach (Picture photo in photos)
                    {
                        BitmapImage sourceImage = new BitmapImage();
                        sourceImage.SetSource(photo.GetThumbnail());
                        WriteableBitmap sourceBitmap = new WriteableBitmap(sourceImage);
                        Color averageColor = sourceBitmap.GetAverageColor();
                        if (!photoColorDB.PhotoColors.Any(x => x.PhotoName != photo.Name))
                        {
                            var newPhotoMetadata = new PhotoMetadata { PhotoName = photo.Name, PhotoColor = averageColor.ToString() };
                            photoColorDB.PhotoColors.InsertOnSubmit(newPhotoMetadata);
                            this.photoDictionary.Add(newPhotoMetadata);
                        }
                    }

                    photoColorDB.SubmitChanges();
                    GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Database updated", "The database was updated.", string.Empty, 0);
                }
                else
                {
                    this.photoDictionary = photoColorDB.PhotoColors.ToList<PhotoMetadata>();
                }
            }
        }

        private void InitialiseCommands()
        {
            this.OpenPhotoCommand = new RelayCommand(() =>
                {
                    PhotoChooserTask photoChooser = new PhotoChooserTask { ShowCamera = true };
                    photoChooser.Completed += PickImageCallback;
                    photoChooser.Show();
                });

            this.SavePhotoCommand = new RelayCommand(() =>
            {
                this.Message = Resources.AppResources.SavingPhotoText;
                this.IsBusy = true;
                this.modifiedFileName = "KollageBurst_" + DateTime.Now.ToString("ddMMyyyy_HHmm");
                this.ModifiedPhoto.PhotoImage.SaveToMediaLibrary(this.modifiedFileName);
                MessageBox.Show(Resources.AppResources.PhotoSavedConfirmationText);
                this.IsBusy = false;
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Photo saved", "User saved photo.", string.Empty, 0);
            },
                () => this.ModifiedPhoto != null);

            this.ShowSettingsCommand = new RelayCommand(() =>
            {
                Messenger.Default.Send<NavigateToPageMessage>(new NavigateToPageMessage("SettingsPage"));
            });

            this.SharePhotoCommand = new RelayCommand(() =>
            {
                if (String.IsNullOrEmpty(this.modifiedFileName))
                {
                    this.SavePhotoCommand.Execute(null);
                }

                ShareMediaTask shareMediaTask = new ShareMediaTask();
                shareMediaTask.FilePath = System.IO.Path.Combine(this.SavePicturesPath, this.modifiedFileName) + ".jpg";
                shareMediaTask.Show();
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Photo shared", "User shared photo.", string.Empty, 0);
            },
                () => this.ModifiedPhoto != null);

            this.ShowAboutPageCommand = new RelayCommand(() =>
            {
                MessageBox.Show("Created by beetrootSOUP");
            });
        }

        private async void PickImageCallback(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                try
                {
                    this.Message = Resources.AppResources.ProcessingPhotoText;
                    this.IsBusy = true;
                    this.OriginalPhoto = new Photo(e.ChosenPhoto);
                    this.ModifiedPhoto = await ProcessBitmap();
                }
                catch (Exception ex)
                {
                    GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.Message, false);
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private async Task<Photo> ProcessBitmap()
        {
            WriteableBitmap originalImage = new WriteableBitmap(this.OriginalPhoto.PhotoImage);
            WriteableBitmap processedImage = new WriteableBitmap(originalImage.PixelWidth, originalImage.PixelHeight);
            MediaLibrary library = new MediaLibrary();

            int horizontalResolution = this.HorizontalResolution;
            int verticalResolution = this.VerticalResolution;
            int panelWidth = (int)(processedImage.PixelWidth / horizontalResolution);
            int panelHeight = (int)(processedImage.PixelHeight / verticalResolution);
            DateTime startTime = DateTime.Now;
            await Task.Run(() =>
                {
                    double originalRatio = originalImage.PixelWidth / (double)originalImage.PixelHeight;
                    for (int i = 0; i < verticalResolution; i++)
                    {
                        for (int j = 0; j < horizontalResolution; j++)
                        {
                            int top = panelHeight * i;
                            int left = panelWidth * j;
                            Color averageColor = originalImage.GetAverageColor(top, left, panelHeight, panelWidth);

                            //// uses tinted original image
                            if (this.UseOriginalPhoto)
                            {
                                processedImage.Blit(new Rect(left, top, panelWidth, panelHeight), originalImage, new Rect(0, 0, originalImage.PixelWidth, originalImage.PixelHeight), averageColor, WriteableBitmapExtensions.BlendMode.None);
                            }
                            else
                            {
                                //// uses photo from library
                                //// cos sie jebie. zoptymalizowac to:
                                //// jak przechowywac dane w bazie? jak porownywac by bylo bardziej efektywnie (bez konwersji do koloru moze)
                                //// moze pozbyc sie klasy photo ? dorzucic tylko etension methods 

                                try
                                {
                                    var nearestColor = averageColor.FindNearestColorMatch(this.photoDictionary.Select(x => x.PhotoColor));
                                    var photo = library.Pictures.Single(x => x.Name == this.photoDictionary.First(y => y.PhotoColor == nearestColor.ToString()).PhotoName);
                                    Action putImage = delegate()
                                    {
                                        BitmapImage sourceImage = new BitmapImage();
                                        sourceImage.SetSource(photo.GetImage());
                                        WriteableBitmap matchedPicture = new WriteableBitmap(sourceImage);
                                        //// zle skaluje powinno brac pod uwage orginalne i  przetworzone obrazy. kwadraty?
                                        double sourceRatio = matchedPicture.PixelWidth / (double)matchedPicture.PixelHeight;
                                        double selectedWidth;
                                        double selectedHeight;
                                        double selectedX = 0;
                                        double selectedY = 0;

                                        if (Math.Abs(originalRatio - sourceRatio) < 0.1)
                                        {
                                            selectedWidth = matchedPicture.PixelWidth;
                                            selectedHeight = matchedPicture.PixelHeight;
                                        }
                                        else if (originalRatio > sourceRatio)
                                        {
                                            selectedWidth = matchedPicture.PixelWidth;
                                            selectedHeight = Enumerable.Min(new[] { matchedPicture.PixelWidth * sourceRatio, matchedPicture.PixelHeight });
                                            selectedY = (matchedPicture.PixelHeight - selectedHeight) / 2;
                                        }
                                        else
                                        {
                                            selectedWidth = Enumerable.Min(new[] { matchedPicture.PixelHeight / sourceRatio, matchedPicture.PixelWidth });
                                            selectedHeight = matchedPicture.PixelHeight;
                                            selectedX = (matchedPicture.PixelWidth - selectedWidth) / 2;
                                        }

                                        var sourceRect = new Rect(selectedX, selectedY, selectedWidth, selectedHeight);

                                        processedImage.Blit(new Rect(left, top, panelWidth, panelHeight), matchedPicture, sourceRect, WriteableBitmapExtensions.BlendMode.None);
                                    };

                                    Deployment.Current.Dispatcher.BeginInvoke(putImage);
                                }
                                catch (Exception ex)
                                {
                                    GoogleAnalytics.EasyTracker.GetTracker().SendException(ex.Message, false);
                                    MessageBox.Show(ex.Message);
                                }

                            }
                        }
                    }
                });

            if (this.UseOriginalPhoto)
            {

                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Collage created", "Created from original.", "Processing time: " + DateTime.Now.Subtract(startTime).ToString(), 0);
            }
            else
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Collage created", "Created from library.", "Processing time: " + DateTime.Now.Subtract(startTime).ToString(), 0);
            }

            this.IsBusy = false;
            return new Photo(processedImage);
        }

        #region App settings methods

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AddOrUpdateValue(string Key, Object value)
        {
            bool valueChanged = false;

            // If the key exists
            if (settings.Contains(Key))
            {
                // If the value has changed
                if (settings[Key] != value)
                {
                    // Store the new value
                    settings[Key] = value;
                    this.settings.Save();
                    valueChanged = true;
                }
            }
            // Otherwise create the key.
            else
            {
                settings.Add(Key, value);
                this.settings.Save();
                valueChanged = true;
            }

            ////GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Settings changed", Key + " set to " + value.ToString(), string.Empty, 0);
            return valueChanged;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private T GetValueOrDefault<T>(string Key, T defaultValue)
        {
            T value;

            // If the key exists, retrieve the value.
            if (settings.Contains(Key))
            {
                value = (T)settings[Key];
            }
            // Otherwise, use the default value.
            else
            {
                value = defaultValue;
            }

            return value;
        }

        #endregion
    }
}