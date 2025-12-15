package com.example.course_project.util;

public class Constants {
    public static final String PREF_NAME = "QualityCheckPrefs";
    public static final String KEY_USER_ID = "user_id";
    public static final String KEY_USER_NAME = "user_name";
    public static final int CHECK_STATUS_PENDING = 0;
    public static final int CHECK_STATUS_COMPLETED = 1;
    public static final int CHECK_STATUS_FAILED = 2;

    public static final String[] QUALITY_CRITERIA = {
            "Свежесть",
            "Срок годности",
            "Внешний вид",
            "Запах",
            "Вкус"
    };

    public static final String[] PRODUCT_CATEGORIES = {
            "Молочные продукты",
            "Мясные продукты",
            "Овощи и фрукты",
            "Хлебобулочные изделия",
            "Напитки",
            "Бакалея"
    };
    public static final int MIN_RATING = 1;
    public static final int MAX_RATING = 5;

    public static final String BASE_URL = "http://10.0.2.2:8080/api/"; 
    public static final String API_TIMEOUT = "30";
    public static final int REQUEST_CAMERA_PERMISSION = 100;
    public static final int REQUEST_GALLERY_PERMISSION = 101;
    public static final int REQUEST_SCAN_BARCODE = 200;
}
