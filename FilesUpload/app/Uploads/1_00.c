#include <stdio.h>
#include <stdlib.h>
#include <time.h>
#include <string.h>

// Mock definitions
#define SERVER_URL "http://iot.server/api/data"
#define DEVICE_ID "IoT-Device-001"
#define SIMULATION_INTERVAL 5 // seconds

// Function to simulate temperature reading
float readTemperature() {
    return 20.0 + (rand() % 100) / 10.0; // Simulate between 20.0°C and 30.0°C
}

// Function to simulate humidity reading
float readHumidity() {
    return 40.0 + (rand() % 600) / 10.0; // Simulate between 40.0% and 100.0%
}

// Function to send data to server (mock)
void sendDataToServer(float temperature, float humidity) {
    // Simulate sending data
    printf("Sending data to server...\n");
    printf("Server URL: %s\n", SERVER_URL);
    printf("Device ID: %s\n", DEVICE_ID);
    printf("Temperature: %.2f°C\n", temperature);
    printf("Humidity: %.2f%%\n", humidity);
    printf("Data sent successfully.\n\n");
}

int main() {
    printf("IoT Device Simulator\n");
    printf("=====================\n");

    // Seed random number generator
    srand(time(NULL));

    // Main loop to simulate data collection and transmission
    while (1) {
        // Read sensor data
        float temperature = readTemperature();
        float humidity = readHumidity();

        // Log the readings
        printf("Collected Data:\n");
        printf("Temperature: %.2f°C\n", temperature);
        printf("Humidity: %.2f%%\n", humidity);

        // Send the data to the server
        sendDataToServer(temperature, humidity);

        // Wait for the next simulation interval
        printf("Waiting for %d seconds...\n\n", SIMULATION_INTERVAL);
        sleep(SIMULATION_INTERVAL);
    }

    return 0;
}


#include <stdio.h>
#include <stdlib.h>
#include <curl/curl.h>

size_t WriteData(void *ptr, size_t size, size_t nmemb, FILE *stream) {
    return fwrite(ptr, size, nmemb, stream);
}

int main() {
    CURL *curl;
    FILE *fp;
    CURLcode res;
    const char *url = "http://yourserver/api/Files/latest";
    const char *outFileName = "new_code.c";

    curl = curl_easy_init();
    if (curl) {
        fp = fopen(outFileName, "wb");
        curl_easy_setopt(curl, CURLOPT_URL, url);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, WriteData);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, fp);
        res = curl_easy_perform(curl);
        if (res != CURLE_OK) {
            fprintf(stderr, "curl_easy_perform() failed: %s\n", curl_easy_strerror(res));
        }
        fclose(fp);
        curl_easy_cleanup(curl);
        printf("File downloaded successfully: %s\n", outFileName);
    } else {
        fprintf(stderr, "Failed to initialize curl\n");
    }
    return 0;
}
