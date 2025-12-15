package com.example.course_project.models;

public class Product {
    private String id;
    private String barcode;
    private String name;
    private String category;
    private String manufacturer;

    public Product(String id, String barcode, String name, String category, String manufacturer) {
        this.id = id;
        this.barcode = barcode;
        this.name = name;
        this.category = category;
        this.manufacturer = manufacturer;
    }
    public String getId() { return id; }
    public void setId(String id) { this.id = id; }

    public String getBarcode() { return barcode; }
    public void setBarcode(String barcode) { this.barcode = barcode; }

    public String getName() { return name; }
    public void setName(String name) { this.name = name; }

    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }

    public String getManufacturer() { return manufacturer; }
    public void setManufacturer(String manufacturer) { this.manufacturer = manufacturer; }
}
