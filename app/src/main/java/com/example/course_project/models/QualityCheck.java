package com.example.course_project.models;

public class QualityCheck {
    private String id;
    private String productId;
    private String date;
    private int rating;
    private String comment;

    public QualityCheck(String id, String productId, String date, int rating, String comment) {
        this.id = id;
        this.productId = productId;
        this.date = date;
        this.rating = rating;
        this.comment = comment;
    }

    public String getId() { return id; }
    public void setId(String id) { this.id = id; }

    public String getProductId() { return productId; }
    public void setProductId(String productId) { this.productId = productId; }

    public String getDate() { return date; }
    public void setDate(String date) { this.date = date; }

    public int getRating() { return rating; }
    public void setRating(int rating) { this.rating = rating; }

    public String getComment() { return comment; }
    public void setComment(String comment) { this.comment = comment; }
}
