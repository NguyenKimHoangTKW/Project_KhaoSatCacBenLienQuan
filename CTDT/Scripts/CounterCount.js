var visitorHub = $.connection.visitorHub;

        visitorHub.client.updateVisitorCount = function (count) {
            $('#visitorCount').text(count);
        };


        $.connection.hub.start().done(function () {
            console.log('SignalR connected');
        });