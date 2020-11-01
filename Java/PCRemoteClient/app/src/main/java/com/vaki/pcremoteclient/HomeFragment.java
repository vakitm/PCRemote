package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

import io.resourcepool.ssdp.client.SsdpClient;
import io.resourcepool.ssdp.model.DiscoveryListener;
import io.resourcepool.ssdp.model.DiscoveryRequest;
import io.resourcepool.ssdp.model.SsdpRequest;
import io.resourcepool.ssdp.model.SsdpService;
import io.resourcepool.ssdp.model.SsdpServiceAnnouncement;

public class HomeFragment extends Fragment implements View.OnClickListener  {
    Button btn;
    View rootView;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        rootView = inflater.inflate(R.layout.fragment_home, container, false);
        btn = (Button) rootView.findViewById(R.id.butt);
        btn.setText("erikci");
        btn.setOnClickListener(this);
        /*btn.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                Log.d("Homefragment","clicked()");
                ((MainActivity) getActivity()).mTcpClient.sendMessage("asd");
            }
        });
        /*private View.OnClickListener buttonClickListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {

            }
        };*/
        Log.d("Homefragment","OnCreateView()");
        return rootView;
    }

    @Override
    public void onClick(View view) {
        Log.d("HomeFragment","clicked");
        ((MainActivity) getActivity()).mTcpClient.sendMessage("asd");
        SsdpClient client = SsdpClient.create();
        DiscoveryRequest all = SsdpRequest.discoverAll();
        client.discoverServices(all, new DiscoveryListener() {
            @Override
            public void onServiceDiscovered(SsdpService service) {
                Log.d("SSDP","Found service: " + service);
            }

            @Override
            public void onServiceAnnouncement(SsdpServiceAnnouncement announcement) {
                Log.d("SSDP","Service announced something: " + announcement);
            }

            @Override
            public void onFailed(Exception e) {

            }
        });

    }
}