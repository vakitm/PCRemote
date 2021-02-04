package com.vaki.pcremoteclient;

import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.SystemClock;
import android.util.Log;
import android.util.Patterns;

import androidx.preference.PreferenceManager;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.InputStreamReader;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.util.regex.Pattern;

public class TcpClient {

    public static final String TAG = TcpClient.class.getSimpleName();
    public static final String SERVER_IP = "192.168.1.2"; //server IP address
    // message to send to the server
    private String mServerMessage;
    // sends message received notifications
    private OnMessageReceived mMessageListener = null;
    // while this is true, the server will continue running
    private boolean mRun = false;
    private boolean allRun = true;
    // used to send messages
    private PrintWriter mBufferOut;
    // used to read messages from the server
    private BufferedReader mBufferIn;

    public MainActivity mainActivity;

    private SharedPreferences SP;
    private static final Pattern PORT_REGEX = Pattern.compile(
            "^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$");

    public TcpClient(OnMessageReceived listener, MainActivity parent) {
        mMessageListener = listener;
        mainActivity = parent;
        SP = PreferenceManager.getDefaultSharedPreferences(mainActivity.getBaseContext());
    }

    public void sendMessage(final String message) {
        Runnable runnable = new Runnable() {
            @Override
            public void run() {

                if (mBufferOut != null) {
                    Log.d(TAG, "Sending: " + message);
                    mBufferOut.write(message);
                    mBufferOut.flush();
                }
            }
        };
        Thread thread = new Thread(runnable);
        thread.start();
    }

    public void stopClient() {

        mRun = false;

        if (mBufferOut != null) {
            mBufferOut.flush();
            mBufferOut.close();
        }

        mMessageListener = null;
        mBufferIn = null;
        mBufferOut = null;
        mServerMessage = null;
        allRun = false;
    }

    public void run() {

        mRun = true;
        while (allRun) {
            try {

                while (!SP.contains("ipaddress")) {
                    mainActivity.changeStatusBar(3);
                    SystemClock.sleep(500);
                }
                while (!Patterns.IP_ADDRESS.matcher(SP.getString("ipaddress", "1")).matches()) {
                    mainActivity.changeStatusBar(4);
                    SystemClock.sleep(500);
                }
                while (!PORT_REGEX.matcher(SP.getString("port", "1337")).matches()) {
                    mainActivity.changeStatusBar(5);
                    SystemClock.sleep(500);
                }


                InetAddress serverAddr = InetAddress.getByName(SP.getString("ipaddress", "1"));

                Log.d("TCP", "C: Connecting...");

                final Socket socket = new Socket();
                InetSocketAddress sockAdr = new InetSocketAddress(serverAddr, Integer.parseInt(SP.getString("port", "1337")));
                socket.connect(sockAdr, 5000);
                Log.d("TCP", "C: Connected");
                mainActivity.changeStatusBar(1);
                try {

                    //sends the message to the server
                    mBufferOut = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), "UTF-8")), true);

                    //receives the message which the server sends back
                    mBufferIn = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));

                    while (mRun) {

                        mServerMessage = mBufferIn.readLine();
                        if (mServerMessage != null && mMessageListener != null) {
                            //call the method messageReceived from MyActivity class
                            mMessageListener.messageReceived(mServerMessage);
                        } else if (mServerMessage == null) {
                            throw new Exception();
                        }
                    }

                } catch (Exception e) {
                    Log.e("TCP", "S: Error2", e);

                } finally {
                    socket.close();
                    mainActivity.changeStatusBar(2);
                    Log.d("TCP", "C: Disconnected");
                }

            } catch (Exception e) {
                Log.e("TCP", "C: Error1", e);
            }
        }

    }

    public interface OnMessageReceived {
        public void messageReceived(String message);
    }

}