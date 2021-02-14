package com.vaki.pcremoteclient;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.os.Handler;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.InputMethodManager;

import android.widget.Toast;

import org.json.JSONException;
import org.json.JSONObject;

import mehdi.sakout.fancybuttons.FancyButton;

public class InputFragment extends Fragment {
    View touchBoard;
    View rootView;
    FancyButton btn;
    Toast mToast;
    long click[] = {0, 0, 0};
    long lastmovepos[] = {0, 0};
    boolean hold = false;
    Handler mHander = new Handler();
    Runnable runObj = new Runnable() {
        @Override
        public void run() {
            Log.d("postdelay", Math.sqrt(Math.pow(click[1] - lastmovepos[0], 2) + Math.pow(click[2] - lastmovepos[1], 2)) + "");
            if (Math.sqrt(Math.pow(click[1] - lastmovepos[0], 2) + Math.pow(click[2] - lastmovepos[1], 2)) < 50 && hold) {
                JSONObject obj = new JSONObject();
                try {
                    obj.put("a", "rc");
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
            }
        }
    };
    int c = 0;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        mToast = Toast.makeText(getContext(), "", Toast.LENGTH_LONG);
        rootView = inflater.inflate(R.layout.fragment_input, container, false);
        btn = rootView.findViewById(R.id.btn_download);
        btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                InputMethodManager im = (InputMethodManager) getActivity().getSystemService(getContext().INPUT_METHOD_SERVICE);
                if (!im.isAcceptingText()) {

                    im.toggleSoftInput(InputMethodManager.SHOW_FORCED, InputMethodManager.HIDE_IMPLICIT_ONLY);
                } else {
                    ((MainActivity) getActivity()).closeSoftKeyb();
                }
            }
        });
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
                            //mToast.setText("ACTION_DOWN AT COORDS " + "X: " + X + " Y: " + Y);
                            //mToast.show();
                            obj.put("a", "d");
                            obj.put("x", X);
                            obj.put("y", Y);
                            click[0] = System.currentTimeMillis();
                            click[1] = X;
                            click[2] = Y;
                            lastmovepos[0] = X;
                            lastmovepos[1] = Y;
                            hold = true;
                            ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            mHander.postDelayed(runObj, 1500);
                            break;

                        case MotionEvent.ACTION_MOVE:
                            if (c % 3 == 0) {
                                //mToast.setText("MOVE " + "X: " + X + " Y: " + Y);
                                //mToast.show();
                                obj.put("a", "m");
                                obj.put("x", X);
                                obj.put("y", Y);
                                lastmovepos[0] = X;
                                lastmovepos[1] = Y;

                                ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            }
                            c++;
                            break;
                        case MotionEvent.ACTION_UP:
                            //mToast.setText("ACTION_UP " + "X: " + X + " Y: " + Y);
                            //mToast.show();
                            mHander.removeCallbacks(runObj);
                            hold = false;
                            if (System.currentTimeMillis() - click[0] < 500 && Math.sqrt(Math.pow(click[1] - X, 2) + Math.pow(click[2] - Y, 2)) < 20) {
                                obj.put("a", "lc");
                                ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                            } else {
                                obj.put("a", "u");
                                obj.put("x", X);
                                obj.put("y", Y);
                                ((MainActivity) getActivity()).mTcpClient.sendMessage(obj.toString());
                                c = 0;
                            }
                            break;
                    }
                } catch (JSONException e) {
                }
                return true;
            }
        });
        return rootView;
    }
}