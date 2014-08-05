var app = {};

app.readMenu = function () {
    
    var structure = [];
    var menuItems = document.querySelectorAll('.menu-item a');

    for (var i = 0; i < menuItems.length; i++) {
        var anchorItem = menuItems.item(i);

        var navItem = {};
        navItem.title = anchorItem.innerText;
        navItem.href = anchorItem.href;
        
        structure.push(navItem);
    }

    var msg = {};
    msg.type = 'menu';
    msg.payload = JSON.stringify(structure);

    framework.scriptNotify(JSON.stringify(msg));
}