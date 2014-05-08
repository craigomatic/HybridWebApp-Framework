/*
* Framework class containing helpers for interacting with the DOM
**/

window.onerror = function (message, url, lineNumber) {
    var msg = {};
    msg.type = 'error';
    msg.payload = JSON.stringify({ message: message, url: url, lineNumber: lineNumber });

    framework.scriptNotify(JSON.stringify(msg));

    return true;
};

var framework = {};

framework.messageProxyId = "hwaf-messageproxy";
framework.useMessageProxy = false;

framework.postDom = function () {
    var msg = {};
    msg.type = 'body';
    msg.payload = document.body.innerHTML;

    framework.scriptNotify(JSON.stringify(msg));
};

framework.hideElement = function (elementId) {
    var toHide = document.querySelector(elementId);

    if (toHide != null) {
        toHide.style.display = "none";
        return true;
    }

    return false;
}

framework.removeElement = function (elementId) {
    var toRemove = document.querySelector(elementId);

    if(toRemove != null) {
        toRemove.parentNode.removeChild(toRemove);
        return true;
    }

    return false;
}

framework.routeTo = function (href) {
    try {
        document.location.href = href;
    }
    catch (exception) {
        window.onerror(exception.message, hash, exception.number);
    }
}


//Enable gesture monitoring in the browser so that the host application can act on them - code adapted from http://www.exploretouch.ie/behind-the-scenes/

framework.enableGestures = function (gestureSurface) {
    var surface = document.querySelector(gestureSurface);
    var msGesture = new MSGesture();
    msGesture.target = surface;

    var MIN_ROTATION = 20;
    var MIN_SCALE = .3;
    var MIN_SWIPE_DISTANCE = 10;

    var rotation, scale, recognised, translationX, translationY;

    surface.addEventListener('MSPointerDown', function (event) {
        msGesture.addPointer(event.pointerId);
    });

    //reset gesture on start
    surface.addEventListener('MSGestureStart', function(event) {
        rotation = 0;
        scale = 1;
        translationX = translationY = 0;
        recognised = false;
    });

    surface.addEventListener('MSGestureChange', function(event) {
        // Disable built-in inertia
        if (event.detail == event.MSGESTURE_FLAG_INERTIA) {
            return;
        }

        // Update total values
        rotation += event.rotation * 180 / Math.PI;
        scale *= event.scale;
        translationX += event.translationX;
        translationY += event.translationY;

        // Try to detect a gesture when not recognised
        if (!recognised) {
            var direction;
            var gesture = {};

            // Check for horizontal swipe
            if (Math.abs(translationX) > MIN_SWIPE_DISTANCE) {
                recognised = true;
                
                direction = translationX < 0 ? 'left' : 'right';

                gesture.type = 'swipe';
                gesture.direction = 'horizontal';
                gesture.value = translationX;

                // Check for vertical swipe
            } else if (Math.abs(translationY) > MIN_SWIPE_DISTANCE) {
                recognised = true;
                
                direction = translationY < 0 ? 'up' : 'down'

                gesture.type = 'swipe';
                gesture.direction = 'vertical';
                gesture.value = translationY;
            }

            // Check for rotation
            if (Math.abs(rotation) >= MIN_ROTATION) {
                recognised = true;

                gesture.type = 'rotate';
                gesture.direction = rotation < 0 ? 'left' : 'right';
                gesture.value = rotation;

                // Check for Pinch/Stretch
            } else if (Math.abs(scale - 1) > MIN_SCALE) {
                recognised = true;

                gesture.type = 'scale';
                gesture.direction = scale > 1 ? 'stretch' : 'pinch';
                gesture.value = scale;
            }

            if (recognised) {
                var msg = {};
                msg.type = 'gesture';
                msg.payload = JSON.stringify(gesture);

                framework.scriptNotify(JSON.stringify(msg));
            }
        }
    });
}

framework.fillInput = function (value, elementId) {
    var toFill = document.querySelector(elementId);

    if (toFill != null) {
        toFill.value = value;
    }
}

framework.clickButton = function (elementId) {
    var toClick = document.querySelector(elementId);

    if (toClick != null) {
        toClick.onclick.call(toClick);
    }
}

framework.hasMatch = function (selector) {
    var toFind = document.querySelector(selector);

    if (toFind != null) {
        return "true";
    }
        
    return "false";
}

framework.injectMessageProxy = function () {

    var msgProxy = document.createElement('iframe');

    msgProxy.frameBorder = 0;
    msgProxy.width = "1px";
    msgProxy.height = "1px";
    msgProxy.id = framework.messageProxyId;
    msgProxy.setAttribute("src", "http://localhost/hwaf/");

    document.documentElement.appendChild(msgProxy);

    framework.useMessageProxy = true;
}

framework.scriptNotify = function (json) {

    //calling app defines if message should be passed via iframe or window.external.notify

    if(framework.useMessageProxy) {
        //place the message object into the src of the iframe as json params
        var iframe = document.querySelector("#" + framework.messageProxyId);
        iframe.setAttribute("src", "http://localhost/hwaf/" + encodeURIComponent(json));
    }
    else {
        window.external.notify(json);
    }
}

framework.appendCss = function (cssString) {
    var css = document.createElement("style");
    css.innerHTML = cssString;
    document.documentElement.appendChild(css);
}

framework.overrideWindowOpen = function () {

    //redefine window.open so that it can be handled by the host app
    window.open = function () {
        var navItem = {};
        navItem.href = arguments[0];

        var msg = {};
        msg.type = 'window.open';
        msg.payload = JSON.stringify(navItem);

        framework.scriptNotify(JSON.stringify(msg));
    }
}