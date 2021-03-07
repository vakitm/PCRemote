package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.os.Handler;
import android.os.Message;
import android.text.format.DateFormat;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import io.resourcepool.ssdp.client.SsdpClient;
import io.resourcepool.ssdp.model.DiscoveryListener;
import io.resourcepool.ssdp.model.DiscoveryRequest;
import io.resourcepool.ssdp.model.SsdpRequest;
import io.resourcepool.ssdp.model.SsdpService;
import io.resourcepool.ssdp.model.SsdpServiceAnnouncement;

public class HomeFragment extends Fragment {
    private View rootView, view;
    private TextView cpu, ram, ping, download, upload;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        rootView = inflater.inflate(R.layout.fragment_home, container, false);
        cpu = rootView.findViewById(R.id.cpu);
        ram = rootView.findViewById(R.id.ram);
        ping = rootView.findViewById(R.id.ping);
        download = rootView.findViewById(R.id.download);
        upload = rootView.findViewById(R.id.upload);
        view = rootView.findViewById(R.id.view);

        Log.d("Homefragment", "OnCreateView()");

        cpu.setText(getString(R.string.cpu_text,"-%" ));
        ram.setText(getString(R.string.ram_text,"-%" ));
        ping.setText(getString(R.string.ping_text,"-" ));
        download.setText(getString(R.string.dl_text,"-" ));
        upload.setText(getString(R.string.ul_text,"-" ));
        return rootView;
    }
    final Handler handler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            Log.d("handler", "get " + msg.obj);
            if (msg.what == 1) {
                try {
                    JSONObject jObject = new JSONObject((String) msg.obj);
                    cpu.setText(getString(R.string.cpu_text,jObject.getInt("cpu")+"%" ));
                    ram.setText(getString(R.string.ram_text,jObject.getInt("ram")+"%" ));
                    ping.setText(getString(R.string.ping_text,jObject.getInt("ping")+"" ));
                    download.setText(getString(R.string.dl_text,jObject.getInt("down")+"" ));
                    upload.setText(getString(R.string.ul_text,jObject.getInt("up")+"" ));
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
            super.handleMessage(msg);
        }
    };
}