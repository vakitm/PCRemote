package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

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
    }
}