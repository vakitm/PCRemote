package com.vaki.pcremoteclient;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.AsyncTask;
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

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.InterfaceAddress;
import java.net.NetworkInterface;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.Map;

public class SettingsFragment extends PreferenceFragmentCompat {
    Thread ipDiscoveryThread;
    EditTextPreference autoDiscoveryPortPreference;
    private SharedPreferences SP;

    @Override
    public void onCreatePreferences(Bundle savedInstanceState, String rootKey) {
        setPreferencesFromResource(R.xml.root_preferences, rootKey);
        Preference button = getPreferenceManager().findPreference("autodiscovery");
        autoDiscoveryPortPreference = getPreferenceManager().findPreference("discoveryport");
        button.setOnPreferenceClickListener(new Preference.OnPreferenceClickListener() {
            @Override
            public boolean onPreferenceClick(Preference preference) {
                Log.d("ssdp", "clicked");

                ipDiscoveryThread = new Thread(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            Looper.prepare();
                            Log.d("ssdp", "run");
                            // Find the server using UDP broadcast
                            //Open a random port to send the package
                            DatagramSocket c = new DatagramSocket();
                            c.setBroadcast(true);

                            byte[] sendData = "PCREMOTE_DISCOVER_REQUEST".getBytes();

                            //Try the 255.255.255.255 first
                            try {

                                DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, InetAddress.getByName("255.255.255.255"), Integer.parseInt(autoDiscoveryPortPreference.getText()));
                                c.send(sendPacket);
                                Log.d("ssdp", getClass().getName() + ">>> Request packet sent to: 255.255.255.255 (DEFAULT)");
                            } catch (Exception e) {
                            }

                            // Broadcast the message over all the network interfaces
                            Enumeration interfaces = NetworkInterface.getNetworkInterfaces();
                            while (interfaces.hasMoreElements()) {
                                NetworkInterface networkInterface = (NetworkInterface) interfaces.nextElement();

                                if (networkInterface.isLoopback() || !networkInterface.isUp()) {
                                    continue; // Don't want to broadcast to the loopback interface
                                }

                                for (InterfaceAddress interfaceAddress : networkInterface.getInterfaceAddresses()) {
                                    InetAddress broadcast = interfaceAddress.getBroadcast();
                                    if (broadcast == null) {
                                        continue;
                                    }

                                    // Send the broadcast package!
                                    try {
                                        DatagramPacket sendPacket = new DatagramPacket(sendData, sendData.length, broadcast, Integer.parseInt(autoDiscoveryPortPreference.getText()));
                                        c.send(sendPacket);
                                    } catch (Exception e) {
                                    }

                                    Log.d("ssdp", getClass().getName() + ">>> Request packet sent to: " + broadcast.getHostAddress() + "; Interface: " + networkInterface.getDisplayName());
                                }
                            }

                            Log.d("ssdp", getClass().getName() + ">>> Done looping over all network interfaces. Now waiting for a reply!");

                            //Wait for a response
                            byte[] recvBuf = new byte[15000];
                            DatagramPacket receivePacket = new DatagramPacket(recvBuf, recvBuf.length);
                            c.receive(receivePacket);

                            //We have a response
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
        });
    }
    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        SP = PreferenceManager.getDefaultSharedPreferences(getActivity().getBaseContext());
        super.onCreate(savedInstanceState);
    }
}