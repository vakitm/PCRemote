package com.vaki.pcremoteclient;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.widget.Toast;

import androidx.annotation.Nullable;
import androidx.preference.EditTextPreference;
import androidx.preference.Preference;
import androidx.preference.PreferenceFragmentCompat;
import androidx.preference.PreferenceManager;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.net.SocketException;
import java.util.Enumeration;

public class SettingsFragment extends PreferenceFragmentCompat {
    private Thread ipDiscoveryThread;
    private EditTextPreference autoDiscoveryPortPreference;
    private SharedPreferences SP;

    //A https://michieldemey.be/blog/network-discovery-using-udp-broadcast/ linkről másolt programkód részletet használtam alapul az alább található részhez.
    /**
     * Az autómatikus szerverkeresés gombjának a kattintás eseményfigyelője
     */
    private Preference.OnPreferenceClickListener autoServerDiscoveryClickListener = new Preference.OnPreferenceClickListener() {
        @Override
        public boolean onPreferenceClick(Preference preference) {
            Log.d("ssdp", "clicked");

            ipDiscoveryThread = new Thread(new Runnable() {

                @Override
                public void run() {
                    try {
                        Looper.prepare();
                        Log.d("ssdp", "run");
                        DatagramSocket c = new DatagramSocket();
                        c.setBroadcast(true);

                        byte[] sendData = "PCREMOTE_DISCOVER_REQUEST".getBytes();
                        try {

                            DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, InetAddress.getByName("255.255.255.255"), Integer.parseInt(autoDiscoveryPortPreference.getText()));
                            c.send(sendPacket);
                            Log.d("ssdp", getClass().getName() + ">>> Request packet sent to: 255.255.255.255 (DEFAULT)");
                        } catch (Exception e) {
                        }
                        Enumeration interfaces = NetworkInterface.getNetworkInterfaces();

                        sendPackets(interfaces,sendData,c);

                        Log.d("ssdp", getClass().getName() + ">>> Done looping over all network interfaces. Now waiting for a reply!");

                        byte[] recvBuf = new byte[15000];
                        DatagramPacket receivePacket = new DatagramPacket(recvBuf, recvBuf.length);
                        c.receive(receivePacket);

                        Log.d("ssdp", getClass().getName() + ">>> Broadcast response from server: " + receivePacket.getAddress().getHostAddress());
                        String message = new String(receivePacket.getData()).trim();
                        if (message.contains("PCREMOTE_DISCOVER_RESPONSE")) {
                            Log.d("IP found", String.valueOf(receivePacket.getAddress()));

                            Toast.makeText(getContext(), "Server found at: "+String.valueOf(receivePacket.getAddress()).replace("/", "")+":"+message.split(":")[1],
                                    Toast.LENGTH_LONG).show();
                            SharedPreferences.Editor editor = SP.edit();
                            editor.putString("ipaddress", String.valueOf(receivePacket.getAddress()).replace("/", ""));
                            editor.putString("port", message.split(":")[1]);
                            editor.commit();
                            setPreferenceScreen(null);
                            addPreferencesFromResource(R.xml.root_preferences);
                        }
                        c.close();
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            });
            if(ipDiscoveryThread.isAlive()) return true;
            ipDiscoveryThread.start();
            final Handler handler = new Handler(Looper.getMainLooper());
            handler.postDelayed(new Runnable() {
                @Override
                public void run() {
                    if(ipDiscoveryThread.isAlive()) {
                        Toast.makeText(getActivity(), "The server could not be found,\nplease enter the IP address manually",
                                Toast.LENGTH_LONG).show();
                        ipDiscoveryThread.interrupt();
                    }
                }
            }, 5000);
            return true;
        }
        private void sendPackets(Enumeration interfaces,byte[] sendData,DatagramSocket c) throws SocketException {
            while (interfaces.hasMoreElements()) {
                NetworkInterface networkInterface = (NetworkInterface) interfaces.nextElement();

                if (networkInterface.isLoopback() || !networkInterface.isUp()) {
                    continue;
                }

                for (InterfaceAddress interfaceAddress : networkInterface.getInterfaceAddresses()) {
                    InetAddress broadcast = interfaceAddress.getBroadcast();
                    if (broadcast == null) {
                        continue;
                    }
                    try {
                        DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, broadcast, Integer.parseInt(autoDiscoveryPortPreference.getText()));
                        c.send(sendPacket);
                    } catch (Exception e) {
                    }

                    Log.d("ssdp", getClass().getName() + ">>> Request packet sent to: " + broadcast.getHostAddress() + "; Interface: " + networkInterface.getDisplayName());
                }
            }
        }
    };
    @Override
    public void onCreatePreferences(Bundle savedInstanceState, String rootKey) {
        setPreferencesFromResource(R.xml.root_preferences, rootKey);
        Preference button = getPreferenceManager().findPreference("autodiscovery");
        autoDiscoveryPortPreference = getPreferenceManager().findPreference("discoveryport");
        button.setOnPreferenceClickListener(autoServerDiscoveryClickListener);
    }
    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        SP = PreferenceManager.getDefaultSharedPreferences(getActivity().getBaseContext());
        super.onCreate(savedInstanceState);
    }
}