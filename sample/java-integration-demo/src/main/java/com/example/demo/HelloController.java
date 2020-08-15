package com.example.demo;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.web.client.RestTemplate;

@RestController
public class HelloController {
    @Autowired
    RestTemplate restTemplate;

    @GetMapping("/hellodotnet")
    public String HelloDotnet(String url){
        if(url == null || url.isEmpty())
            url = "http://localhost:5001/api/values";
        ResponseEntity<String> resp = restTemplate.getForEntity(url, String.class);
        return  resp.getBody();
    }

    @GetMapping("/sayhello")
    public String SayHello(){
        return "Hello Skywalking!";
    }

}
