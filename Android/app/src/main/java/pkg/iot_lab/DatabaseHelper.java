package pkg.iot_lab;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

import com.github.mikephil.charting.data.Entry;

import java.util.ArrayList;
import java.util.List;

public class DatabaseHelper extends SQLiteOpenHelper {
    private static final String DATABASE_NAME = "sensorData.db";
    private static final int DATABASE_VERSION = 1;

    public static final String TABLE_TEMP = "temperature";
    public static final String TABLE_HUMID = "humidity";
    public static final String TABLE_LIGHT = "light";

    private static final String COLUMN_ID = "id";
    private static final String COLUMN_VALUE = "value";

    public DatabaseHelper(Context context) {
        super(context, DATABASE_NAME, null, DATABASE_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        String CREATE_TEMP_TABLE = "CREATE TABLE " + TABLE_TEMP + "("
                + COLUMN_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + COLUMN_VALUE + " REAL" + ")";
        String CREATE_HUMID_TABLE = "CREATE TABLE " + TABLE_HUMID + "("
                + COLUMN_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + COLUMN_VALUE + " REAL" + ")";
        String CREATE_LIGHT_TABLE = "CREATE TABLE " + TABLE_LIGHT + "("
                + COLUMN_ID + " INTEGER PRIMARY KEY AUTOINCREMENT,"
                + COLUMN_VALUE + " REAL" + ")";

        db.execSQL(CREATE_TEMP_TABLE);
        db.execSQL(CREATE_HUMID_TABLE);
        db.execSQL(CREATE_LIGHT_TABLE);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_TEMP);
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_HUMID);
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_LIGHT);
        onCreate(db);
    }

    public void addSensorData(String tableName, float value) {
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues values = new ContentValues();
        values.put(COLUMN_VALUE, value);
        db.insert(tableName, null, values);
        db.close();
    }

    public List<Entry> getAllSensorData(String tableName) {
        List<Entry> data = new ArrayList<>();
        String selectQuery = "SELECT * FROM " + tableName;
        SQLiteDatabase db = this.getReadableDatabase();
        Cursor cursor = db.rawQuery(selectQuery, null);
        int index = 0;
        if (cursor.moveToFirst()) {
            do {
                data.add(new Entry(index++, cursor.getFloat(cursor.getColumnIndexOrThrow(COLUMN_VALUE))));
            } while (cursor.moveToNext());
        }
        cursor.close();
        db.close();
        return data;
    }
}
