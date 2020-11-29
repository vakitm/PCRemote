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

public class HomeFragment extends Fragment implements View.OnClickListener  {
    Button btn;
    View rootView,view;
    TextView cpu,ram,ping,download,upload;


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        rootView = inflater.inflate(R.layout.fragment_home, container, false);
        cpu = rootView.findViewById(R.id.cpu);
        ram = rootView.findViewById(R.id.ram);
        ping = rootView.findViewById(R.id.ping);
        download = rootView.findViewById(R.id.download);
        upload = rootView.findViewById(R.id.upload);
        view = rootView.findViewById(R.id.view);


       /* btn = (Button)rootView.findViewById(R.id.button);
        btn.setOnClickListener(new View.OnClickListener()
        {
            @Override
            public void onClick(View v)
            {
                InputMethodManager im = (InputMethodManager)getActivity().getSystemService(getContext().INPUT_METHOD_SERVICE);
                im.toggleSoftInput(InputMethodManager.SHOW_FORCED, InputMethodManager.HIDE_IMPLICIT_ONLY);

                Log.d("asd","clicked");
            }
        });*/


        /*btn = (Button) rootView.findViewById(R.id.butt);
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
    final Handler handler = new Handler(){
        @Override
        public void handleMessage(Message msg) {
            Log.d("handler", "get " +msg.obj);
            if(msg.what==1)
            {
                try {
                    JSONObject jObject = new JSONObject((String) msg.obj);
                    cpu.setText("CPU: "+jObject.getInt("cpu")+"%");
                    ram.setText("RAM: "+jObject.getInt("ram")+"%");
                    ping.setText("Ping: "+jObject.getInt("ping")+" ms");
                    download.setText("Download: "+jObject.getInt("down")+" KB/sec");
                    upload.setText("Upload: "+jObject.getInt("up")+" KB/sec");
                }
                catch (Exception e){ e.printStackTrace();}
            }
            super.handleMessage(msg);
        }
    };
}