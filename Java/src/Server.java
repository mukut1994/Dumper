import java.net.InetAddress;
import java.net.ServerSocket;

public class Server {

    private InetAddress ip;
    private short port;

    public Server(InetAddress ip, short port)
    {
        this.ip = ip;
        this.port = port;
    }

    public Server RunInBackground() {


        new Thread(RunAsync());
        return this;
    }

    public Runnable RunAsync() {
        try {
            ServerSocket serverSocket = new ServerSocket(port, 50, ip);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
