package com.vaki.pcremoteclient;

import androidx.annotation.NonNull;
import androidx.appcompat.app.ActionBarDrawerToggle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.content.res.ResourcesCompat;
import androidx.core.view.GravityCompat;
import androidx.drawerlayout.widget.DrawerLayout;

import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.google.android.material.navigation.NavigationView;

import java.util.Timer;
import java.util.TimerTask;

import io.resourcepool.ssdp.client.SsdpClient;
import io.resourcepool.ssdp.model.DiscoveryListener;
import io.resourcepool.ssdp.model.DiscoveryRequest;
import io.resourcepool.ssdp.model.SsdpRequest;
import io.resourcepool.ssdp.model.SsdpService;
import io.resourcepool.ssdp.model.SsdpServiceAnnouncement;

public class MainActivity extends AppCompatActivity implements NavigationView.OnNavigationItemSelectedListener {

    private DrawerLayout drawer;
    TcpClient mTcpClient;
    public LinearLayout statusbar;
    public TextView statusbar_text;
    public Timer timerRef;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        drawer = findViewById(R.id.drawer_layout);

        statusbar = (LinearLayout) findViewById(R.id.statusbar);
        statusbar_text = (TextView)findViewById(R.id.statusbar_text) ;

        NavigationView navigationView = findViewById(R.id.nav_view);
        navigationView.setNavigationItemSelectedListener(this);
        ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar,
                R.string.navigation_drawer_open, R.string.navigation_drawer_close);
        drawer.addDrawerListener(toggle);
        toggle.syncState();

        if (savedInstanceState == null) {
            getSupportFragmentManager().beginTransaction().replace(R.id.fragment_container, new HomeFragment()).commit();
            navigationView.setCheckedItem(R.id.nav_home);
        }
        AsyncTask ConnectTask = new ConnectTask(this);

        //new ConnectTask().execute("");
    }
    public void changeStatusBar(final int status)
    {
        runOnUiThread(new Runnable() {

            @Override
            public void run() {
                switch(status)
                {
                    case 1:
                        statusbar.setVisibility(View.VISIBLE);
                        statusbar.setBackgroundColor(ResourcesCompat.getColor(getResources(), R.color.connected, null));
                        statusbar_text.setText("Connected");
                        timerRef = new Timer();
                        timerRef.schedule(new TimerTask() {
                            @Override
                            public void run() {
                                Message msg = handler.obtainMessage();
                                msg.what = 1;
                                handler.sendMessage(msg);
                            }
                        }, 3000);
                        break;
                    case 2:
                        statusbar.setVisibility(View.VISIBLE);
                        statusbar.setBackgroundColor(ResourcesCompat.getColor(getResources(), R.color.connecting, null));
                        statusbar_text.setText("Connection Lost, Reconnecting");
                        timerRef.cancel();
                        break;
                }
            }
        });

    }
    final Handler handler = new Handler(){
        @Override
        public void handleMessage(Message msg) {
            if(msg.what==1)
            {
                try {
                    statusbar.setVisibility(View.GONE);
                }
                catch (Exception e){}
            }
            super.handleMessage(msg);
        }
    };
    @Override
    public void onBackPressed() {
        if (drawer.isDrawerOpen(GravityCompat.START)) {
            drawer.closeDrawer(GravityCompat.START);
        } else {
            super.onBackPressed();
        }
    }

    @Override
    public boolean onNavigationItemSelected(@NonNull MenuItem menuItem) {
        switch (menuItem.getItemId()) {
            case R.id.nav_home:
                getSupportFragmentManager().beginTransaction().replace(R.id.fragment_container, new HomeFragment()).commit();
                break;
            case R.id.nav_settings:
                getSupportFragmentManager().beginTransaction().replace(R.id.fragment_container, new SettingsFragment()).commit();
                break;

        }
        drawer.closeDrawer(GravityCompat.START);
        return true;
    }

    public class ConnectTask extends AsyncTask<String, String, TcpClient> {

        MainActivity mainActivity;
        public ConnectTask(MainActivity activity)
        {
            mainActivity =activity;
            this.execute("");
        }
        @Override
        protected TcpClient doInBackground(String... message) {

            //we create a TCPClient object
            mTcpClient = new TcpClient(new TcpClient.OnMessageReceived() {
                @Override
                //here the messageReceived method is implemented
                public void messageReceived(String message) {
                    //this method calls the onProgressUpdate
                    publishProgress(message);
                }
            },mainActivity);
            mTcpClient.run();

            return null;
        }

        @Override
        protected void onProgressUpdate(String... values) {
            super.onProgressUpdate(values);
            Log.d("TCP Client", "response " + values[0]);
        }
    }
}

