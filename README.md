# Google Drive access using Xamarin.Android and Xamarin.GooglePlayService.Drive component

Google Drive access using Xamarin.Android and Xamarin.GooglePlayService.Drive component

Before starting, make sure that you have performed the following steps:

 1. Enable Google Drive on the Google API developer console.
 2. Create credentials using the package name and SHA-1 signature
 
         - For debug, the SHA-1 must be taken from the debug keystore used by Xamarin to sign the app
           `keytool -list -v -keystore ~/.local/share/Xamarin/Mono\ for\ Android/debug.keystore -alias androiddebugkey -storepass android -keypass android`
           
         - For release, the SHA-1 must be taken from the keystore used to sign the app

## Useful links

- [GDrive Google doc - https://developers.google.com/drive/android/auth](https://developers.google.com/drive/android/auth)
- [Xamarin Drive component - https://components.xamarin.com/gettingstarted/googleplayservices-drive](https://components.xamarin.com/gettingstarted/googleplayservices-drive)
- [Find your SHA-1 (Xamarin) - https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/MD5_SHA1/](https://developer.xamarin.com/guides/android/deployment,_testing,_and_metrics/MD5_SHA1/)
