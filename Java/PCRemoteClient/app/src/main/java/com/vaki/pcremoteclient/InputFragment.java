package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

public class InputFragment extends Fragment {
    View touchBoard;
    View rootView;
    int c = 0;
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        rootView = inflater.inflate(R.layout.fragment_input, container, false);
        touchBoard = rootView.findViewById(R.id.touchBoard);
        touchBoard.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {

                int X = (int) event.getX();
                int Y = (int) event.getY();
                int eventaction = event.getAction();
                JSONObject obj = new JSONObject();

                try {
                    switch (eventaction) {
                        case MotionEvent.ACTION_DOWN:
                            Toast.makeText(getActivity(), "ACTION_DOWN AT COORDS "+"X: "+X+" Y: "+Y, Toast.LENGTH_SHORT).show();
                            obj.put("a", "d");
                            obj.put("x", X);
                            obj.put("y", Y);
                            ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            break;

                        case MotionEvent.ACTION_MOVE:
                            if(c%3==0) {
                                Toast.makeText(getActivity(), "MOVE " + "X: " + X + " Y: " + Y, Toast.LENGTH_SHORT).show();
                                obj.put("a", "m");
                                obj.put("x", X);
                                obj.put("y", Y);

                                ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            }
                            c++;
                            break;
                        case MotionEvent.ACTION_UP:
                            Toast.makeText(getActivity(), "ACTION_UP "+"X: "+X+" Y: "+Y, Toast.LENGTH_SHORT).show();
                            obj.put("a", "u");
                            obj.put("x", X);
                            obj.put("y", Y);
                            ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            break;
                    }
                } catch (JSONException e) {

                }
                /*switch (eventaction) {
                    case MotionEvent.ACTION_DOWN:
                        Toast.makeText(getActivity(), "ACTION_DOWN AT COORDS "+"X: "+X+" Y: "+Y, Toast.LENGTH_SHORT).show();
                        ((MainActivity) getActivity()).mTcpClient.sendMessage("ACTION_DOWN AT COORDS "+"X: "+X+" Y: "+Y);
                        break;

                    case MotionEvent.ACTION_MOVE:
                        Toast.makeText(getActivity(), "MOVE "+"X: "+X+" Y: "+Y, Toast.LENGTH_SHORT).show();
                        ((MainActivity) getActivity()).mTcpClient.sendMessage("MOVE "+"X: "+X+" Y: "+Y);
                        break;
                    case MotionEvent.ACTION_SCROLL:
                        ((MainActivity) getActivity()).mTcpClient.sendMessage("SCROLL");
                        break;
                    case MotionEvent.ACTION_UP:
                        Toast.makeText(getActivity(), "ACTION_UP "+"X: "+X+" Y: "+Y, Toast.LENGTH_SHORT).show();
                        ((MainActivity) getActivity()).mTcpClient.sendMessage("ACTION_UP "+"X: "+X+" Y: "+Y);
                        break;
                }*/
                return true;

            }
        });
        return rootView;
    }
}