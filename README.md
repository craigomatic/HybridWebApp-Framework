HybridWebApp-Framework
======================

A framework to make life easier when creating Hybrid WebApps with Windows Phone 8, Windows Phone 8.1 and Windows 8.1

The framework library currently supports:
* Simple messaging protocol between web context and host app context
* Gesture surfacing (swipe, rotate, pinch/stretch) to the host app 
* Error surfacing to the host app
* Misc DOM helpers

The toolkit library currently provides:
* Reusable [HybridWebView control](https://github.com/craigomatic/HybridWebApp-Framework/wiki/HybridWebView-Control) that simplifies the communcation between web and host contexts
* Facebook and Google authentication helpers
* Default loading and offline overlays

If you're new to the framework, the best place to begin is the [Getting Started](https://github.com/craigomatic/HybridWebApp-Framework/wiki#getting-started) section of the wiki. 

### Download via NuGet

If you already know what you're doing, grab the NuGet package(s) below and start building!

#### Framework

The framework library, you do all the work to wire it up yourself.

[Install-Package HybridWebApp.Framework](https://www.nuget.org/packages/HybridWebApp.Framework/)

#### Toolkit

The toolkit, contains reusable control(s) and implementations of interfaces defined in the HybridWebApp.Framework project. 

[Install-Package HybridWebApp.Toolkit](https://www.nuget.org/packages/HybridWebApp.Toolkit/)

