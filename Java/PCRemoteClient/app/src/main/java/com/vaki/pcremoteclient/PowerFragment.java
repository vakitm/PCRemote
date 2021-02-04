package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Switch;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.HashMap;

import mehdi.sakout.fancybuttons.FancyButton;

public class PowerFragment extends Fragment implements View.OnClickListener {
    View rootView;
    FancyButton btn_shutdown, btn_restart, btn_sleep, btn_hibernate, btn_logoff, btn_lock;

    public PowerFragment() {
    }


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        rootView = inflater.inflate(R.layout.fragment_power, container, false);
        btn_shutdown = rootView.findViewById(R.id.btn_shutdown);
        btn_shutdown.setOnClickListener(this);
        btn_restart = rootView.findViewById(R.id.btn_restart);
        btn_restart.setOnClickListener(this);
        btn_sleep = rootView.findViewById(R.id.btn_sleep);
        btn_sleep.setOnClickListener(this);
        btn_hibernate = rootView.findViewById(R.id.btn_hibernate);
        btn_hibernate.setOnClickListener(this);
        btn_logoff = rootView.findViewById(R.id.btn_logoff);
        btn_logoff.setOnClickListener(this);
        btn_lock = rootView.findViewById(R.id.btn_lock);
        btn_lock.setOnClickListener(this);
        return rootView;
    }

    @Override
    public void onClick(View ve) {
        FancyButton v = (FancyButton) ve;
        JSONObject obj = new JSONObject();
        try {
            switch (v.getId()) {
                case R.id.btn_shutdown:
                    obj.put("a", "sd");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;

                case R.id.btn_restart:
                    obj.put("a", "rs");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;
                case R.id.btn_sleep:
                    obj.put("a", "sl");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;
                case R.id.btn_hibernate:
                    obj.put("a", "hb");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;
                case R.id.btn_logoff:
                    obj.put("a", "lo");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;
                case R.id.btn_lock:
                    obj.put("a", "lk");
                    ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                    break;
            }
        } catch (JSONException e) {
        }
    }
}