HybridWebApp-Framework
======================

A framework to make life easier when creating Hybrid WebApps with Windows Phone 8, Windows Phone 8.1 and Windows 8.1

This library currently supports:
* Simple messaging protocol between web context and host app context
* Gesture surfacing (swipe, rotate, pinch/stretch) to the host app 
* Error surfacing to the host app
* Misc DOM helpers

## Download via NuGet

*New!* A NuGet package has been created for all the supported platforms and is now the recommended way that users install both the framework and the toolkit:

[Install-Package HybridWebApp.Toolkit](https://www.nuget.org/packages/HybridWebApp.Toolkit/)

The toolkit includes a reusable control that makes it even simple to get up and running and is the recommended starting point for new projects.

## Basic Usage

```c#

//BrowserWrapper is an implementation of IBrowser and IScriptInvoker tied either to WebBrowser or WebView (depending on WP8/WP8.1 or Universal App)
var browserWrapper = new BrowserWrapper(WebBrowser);

var interpreter = new Interpreter(browserWrapper);

var webRoute = new WebRoute(interpreter, browserWrapper);
webRoute.Root = new Uri("http://example.org");

//map all route changes so that the framework, app.js and app.css are loaded each time (they are flushed on navigation)
webRoute.Map("/", async (uri, success, errorCode) =>
{
    if (success)
    {
        await _Interpreter.LoadFrameworkAsync();
        await _Interpreter.LoadAsync("ms-appx:///www/js/app.js");
        await _Interpreter.LoadCssAsync("ms-appx:///www/css/app.css");
    }
    else
    {
        //TODO: handle this somehow, ie: show offline overlay
    }
});
```


## Messaging

Messaging back and forward between the webpage and the host application is of critical importance if you want to create a native feeling app. 

With bi-directional messaging, you have the ability to get creative and do things like:

* Dynamically replace the websites navigational structure with a native app structure
* Pin interesting content via secondary tile (ie: recipe, album, tv series, etc)
* Interact with OS features such as the [System Media Transport Controls](http://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn263187.aspx)
* Manipulate the DOM to add new buttons into the website that perform actions in the host app

### Windows Phone 8/Windows Phone 8.1 Silverlight

#### From Host to Client

```C#
  interpreter.Eval("app.doSomething();"); //call a known method defined in app.js
```

#### From Client to Host

In app.js:
```js
  app.doSomething = function () {
    var navItem = {};
    navItem.href = "http://example.org";
    navItem.title = "Homepage";
    
    var msg = {};
    msg.type = 'something';
    msg.payload = JSON.stringify(payload);

    framework.scriptNotify(JSON.stringify(msg));
}
```

In the host app:

```C#

WebBrowser.ScriptNotify += (sender, args) =>
{
    var msg = JsonConvert.DeserializeObject<ScriptMessage>(args.Value);

    switch (msg.Type)
    {
      case "something":
        {
          var navItem = JsonConvert.DeserializeObject<NavItem>(msg.Payload);
          break;
        }
};

```

### Windows Universal App (W8.1 & WP8.1)

The security model changed in Windows Phone 8.1/Windows 8.1 and as a result window.external.notify only functions for known ContentURIs running over __HTTPS__.

An alternative approach is to inject an iFrame into the page and use changes to its src as a messaging channel. framework.js includes helper functions to make this simple.

#### From Host to Client

The same process for WP8/WP8.1 applies here, see above.

#### From Client to Host

```C#

//in the initial mapping code, be sure to overload the LoadFramworkAsync with true so the iFrame channel is created
webRoute.Map("/", async (uri, success, errorCode) =>
{
    if (success)
    {
        await _Interpreter.LoadFrameworkAsync(WebToHostMessageChannel.IFrame);
        
        //etc
    }
    
    //etc
}

//listen for the iFrame navigation event, this is where the messages will be delivered
        
WebBrowser.FrameNavigationStarting += (sender, args) =>
{
    if (!uri.OriginalString.Contains(FrameworkConstants.MessageProxyPath) || uri.OriginalString.EndsWith(FrameworkConstants.MessageProxyPath))
            {
                return;
            }

            //process message
            var encodedMsg = uri.AbsolutePath.Replace(FrameworkConstants.MessageProxyPath, string.Empty);
            var jsonString = System.Net.WebUtility.UrlDecode(encodedMsg).Replace("/\"", "\\\"");
            var msg = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScriptMessage>(jsonString));

    switch (msg.Type)
    {
        case "something":
            {
                var navItem = JsonConvert.DeserializeObject<NavItem>(msg.Payload);
                break;
            }
    }

    //cancel navigation
    args.Cancel = true;
}; 
```



