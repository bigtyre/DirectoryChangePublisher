class SignalRClient {
    constructor(serverUrl, hubPath = "/event-hub") {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(`${serverUrl}${hubPath}`)
            .withAutomaticReconnect()
            .build();

        this.connection.on("ReloadRequested", () => {
            console.log("Received reload request from signalr. Reloading the page.");
            window.location.reload();
        });

        this.connection.on("ReceiveMessage", () => {
            console.log("Received reload request from signalr. Reloading the page.");
            window.location.reload();
        });

        this.connection.onreconnecting((error) => {
            console.warn("SignalR connection lost. Reconnecting...", error);
        });

        // Event for when the connection is successfully reconnected
        this.connection.onreconnected((connectionId) => {
            console.log("SignalR connection reestablished. Connection ID:", connectionId);
        });
        
    }

    async start() {
        try {
            await this.connection.start();
            console.log("SignalR connection established.");
        } catch (err) {
            console.error("Error establishing SignalR connection:", err);
        }
    }
    /*
    async sendMessage(user, message) {
        try {
            await this.connection.invoke("SendMessage", user, message);
            console.log(`Message sent: ${message}`);
        } catch (err) {
            console.error("Error sending message:", err);
        }
    }
    */
    onMessageReceived(callback) {
        this.connection.on("ReloadRequested", callback);
    }

    stop() {
        this.connection.stop().then(() => console.log("SignalR connection stopped."));
    }
}
