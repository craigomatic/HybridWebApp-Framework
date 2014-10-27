var app = {};

app.soundCloudController = null;

function _getController() {

    try
    {
        if (app.soundCloudController == null) {

            var iframeElement = document.querySelector('#soundcloud-iframe');
            app.soundCloudController = SC.Widget(iframeElement);
        }

        return app.soundCloudController;
    }
    catch(e) {
        return null;
    }
}

app.playTrack = function () {
    _getController().play();
}

app.pauseTrack = function () {
    _getController().pause();
}

app.postTrackInfo = function () {

    var controller = null;

    var id = setInterval(function () {
        controller = _getController();

        if(controller != null) {

            controller.getCurrentSound(function (cs) {
                var trackInfo = {};
                trackInfo.album = '';
                trackInfo.artist = cs.user.username;
                trackInfo.title = cs.title;
                trackInfo.imageUri = cs.artwork_url;

                var msg = {};
                msg.type = 'trackInfo';
                msg.payload = JSON.stringify(trackInfo);

                framework.scriptNotify(JSON.stringify(msg));
            });

            clearTimeout(id);
        }
    }, 1000);

    
}