package pkg.iot_lab;

import androidx.appcompat.app.AppCompatActivity;

import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageView;
import android.widget.TextView;

import com.github.angads25.toggle.interfaces.OnToggledListener;
import com.github.angads25.toggle.model.ToggleableView;
import com.github.angads25.toggle.widget.LabeledSwitch;
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.components.XAxis;
import com.github.mikephil.charting.components.YAxis;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;

import org.eclipse.paho.client.mqttv3.IMqttDeliveryToken;
import org.eclipse.paho.client.mqttv3.MqttCallbackExtended;
import org.eclipse.paho.client.mqttv3.MqttMessage;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {
    MQTTHelper mqttHelper;
    TextView txtTemp, txtHumi, txtLight, txtbtn1, txtbtn2;
    LabeledSwitch btn1, btn2;
    LineChart lineChartTemp, lineChartHumid, lineChartLight;
    DatabaseHelper databaseHelper;
    List<Entry> tempData = new ArrayList<>();
    List<Entry> humidData = new ArrayList<>();
    List<Entry> lightData = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        txtTemp = findViewById(R.id.txtTemp);

        btn1 = findViewById(R.id.btn1);


        lineChartTemp = findViewById(R.id.lineChartTemp);
        lineChartHumid = findViewById(R.id.lineChartHumid);
        lineChartLight = findViewById(R.id.lineChartLight);

        databaseHelper = new DatabaseHelper(this);

        // Load existing data from database
        tempData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_TEMP);
        humidData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_HUMID);
        lightData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_LIGHT);

        updateChartData(lineChartTemp, tempData);
        updateChartData(lineChartHumid, humidData);
        updateChartData(lineChartLight, lightData);

        // Set the latest values to the TextViews
        if (!tempData.isEmpty()) {
            txtTemp.setText(tempData.get(tempData.size() - 1).getY() + "°C");
        }
        if (!humidData.isEmpty()) {
            txtHumi.setText(humidData.get(humidData.size() - 1).getY() + "%");
        }
        if (!lightData.isEmpty()) {
            txtLight.setText(lightData.get(lightData.size() - 1).getY() + "");
        }

        startMQTT();

        btn1.setOnToggledListener(new OnToggledListener() {
            @Override
            public void onSwitched(ToggleableView toggleableView, boolean isOn) {
                if (isOn) {
                    mqttHelper.publishData("batrong0610/feeds/nutnhan1", "1");
                } else {
                    mqttHelper.publishData("batrong0610/feeds/nutnhan1", "0");
                }
            }
        });


        View.OnClickListener temperatureClickListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                toggleChartVisibility(lineChartTemp);
            }
        };

        View.OnClickListener humidityClickListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                toggleChartVisibility(lineChartHumid);
            }
        };

        View.OnClickListener lightClickListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                toggleChartVisibility(lineChartLight);
            }
        };

        txtTemp.setOnClickListener(temperatureClickListener);
        findViewById(R.id.img_temperature).setOnClickListener(temperatureClickListener);

        setupChart(lineChartTemp, "Temperature Data");
        setupChart(lineChartHumid, "Humidity Data");
        setupChart(lineChartLight, "Light Data");
    }

    private void toggleChartVisibility(LineChart chart) {
        if (chart.getVisibility() == View.GONE) {
            lineChartTemp.setVisibility(View.GONE);
            lineChartHumid.setVisibility(View.GONE);
            lineChartLight.setVisibility(View.GONE);
            chart.setVisibility(View.VISIBLE);
        } else {
            chart.setVisibility(View.GONE);
        }
    }

    private void setupChart(LineChart chart, String label) {
        XAxis xAxis = chart.getXAxis();
        YAxis yAxisLeft = chart.getAxisLeft();
        YAxis yAxisRight = chart.getAxisRight();

        xAxis.setPosition(XAxis.XAxisPosition.BOTTOM);
        yAxisRight.setEnabled(false);
        chart.getDescription().setEnabled(false);
        chart.getLegend().setEnabled(true);
    }

    private void updateChartData(LineChart chart, List<Entry> data) {
        LineDataSet dataSet = new LineDataSet(data, chart.getDescription().getText().toString());
        LineData lineData = new LineData(dataSet);
        chart.setData(lineData);
        chart.invalidate(); // Refresh chart
    }

    private List<Entry> parseMessageData(String message, List<Entry> existingData) {
        String[] parts = message.split(",");
        int startIndex = existingData.size();
        for (int i = 0; i < parts.length; i++) {
            existingData.add(new Entry(startIndex + i, Float.parseFloat(parts[i].trim())));
        }
        return existingData;
    }

    public void startMQTT() {
        mqttHelper = new MQTTHelper(this);
        mqttHelper.setCallback(new MqttCallbackExtended() {
            @Override
            public void connectComplete(boolean reconnect, String serverURI) {
            }

            @Override
            public void connectionLost(Throwable cause) {
            }

            @Override
            public void messageArrived(String topic, MqttMessage message) throws Exception {
                Log.d("Test", topic + "***" + message.toString());
                if (topic.contains("cambien1")) {
                    float tempValue = Float.parseFloat(message.toString());
                    txtTemp.setText(tempValue + "°C");
                    databaseHelper.addSensorData(DatabaseHelper.TABLE_TEMP, tempValue);
                    tempData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_TEMP);
                    updateChartData(lineChartTemp, tempData);
                } else if (topic.contains("cambien2")) {
                    float humidValue = Float.parseFloat(message.toString());
                    txtHumi.setText(humidValue + "%");
                    databaseHelper.addSensorData(DatabaseHelper.TABLE_HUMID, humidValue);
                    humidData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_HUMID);
                    updateChartData(lineChartHumid, humidData);
                } else if (topic.contains("cambien3")) {
                    float lightValue = Float.parseFloat(message.toString());
                    txtLight.setText(message.toString());
                    databaseHelper.addSensorData(DatabaseHelper.TABLE_LIGHT, lightValue);
                    lightData = databaseHelper.getAllSensorData(DatabaseHelper.TABLE_LIGHT);
                    updateChartData(lineChartLight, lightData);
                } else if (topic.contains("nutnhan1")) {
                    if (message.toString().contains("1")) {
                        btn1.setOn(true);
                    } else {
                        btn1.setOn(false);
                    }
                }
            }

            @Override
            public void deliveryComplete(IMqttDeliveryToken token) {
            }
        });
    }
}
