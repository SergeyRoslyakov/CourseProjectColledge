package com.example.course_project;

import android.os.Bundle;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.navigation.NavController;
import androidx.navigation.Navigation;
import androidx.navigation.ui.AppBarConfiguration;
import androidx.navigation.ui.NavigationUI;

import com.example.course_project.databinding.ActivityMainBinding;
import com.example.course_project.models.Product;
import com.example.course_project.models.QualityCheck;

import java.util.ArrayList;
import java.util.Date;
import java.text.SimpleDateFormat;
import java.util.Locale;

public class MainActivity extends AppCompatActivity {

    private ActivityMainBinding binding;

    private ArrayList<Product> productList = new ArrayList<>();
    private ArrayList<QualityCheck> checkList = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        binding = ActivityMainBinding.inflate(getLayoutInflater());
        setContentView(binding.getRoot());

        // Настройка навигации
        NavController navController = Navigation.findNavController(this, R.id.nav_host_fragment_activity_main);
        AppBarConfiguration appBarConfiguration = new AppBarConfiguration.Builder(
                R.id.navigation_scan,
                R.id.navigation_history,
                R.id.navigation_analytics,
                R.id.navigation_profile
        ).build();
        NavigationUI.setupActionBarWithNavController(this, navController, appBarConfiguration);
        NavigationUI.setupWithNavController(binding.navView, navController);

        initTestData();

        Toast.makeText(this, "Приложение QualityCheck запущено", Toast.LENGTH_SHORT).show();
    }

    private void initTestData() {
        productList.add(new Product("1", "4601234567890", "Молоко 3.2%", "Молочные продукты", "Простоквашино"));
        productList.add(new Product("2", "4601234567891", "Хлеб Бородинский", "Хлебобулочные изделия", "Хлебзавод №1"));

        String currentDate = new SimpleDateFormat("dd.MM.yyyy", Locale.getDefault()).format(new Date());
        checkList.add(new QualityCheck("1", "1", currentDate, 5, "Отличное качество"));
        checkList.add(new QualityCheck("2", "2", currentDate, 4, "Хорошее качество, свежий"));
    }

    public ArrayList<Product> getProductList() {
        return productList;
    }

    public ArrayList<QualityCheck> getCheckList() {
        return checkList;
    }
}