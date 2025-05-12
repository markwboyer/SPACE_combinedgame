using System; // Used for dates and times exclusively
using System.Diagnostics; // Used for stopwatch function
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PFDScript : MonoBehaviour
{
    Transform speed;
    // Transform UTC;
    Transform timer;
    Transform PET_timer;
    public Transform mp_text;
    Transform total_distance;
    // Transform dist_from_base;
    Transform angle;
    Transform dist_to_empty;
    Transform dist_to_empty_bar;
    Transform power;
    // Transform avg_kmh;
    // Transform avg_range;
    Transform power_efficiency;
    Transform power_efficiency_text;
    Transform power_remaining;
    Transform power_remaining_text;
    Transform heading;

    GameObject rover;
    GameObject habitat;
    Rigidbody rover_rb;
    PowerController rover_script;

    public Texture2D red_progress_bar;
    public Texture2D green_progress_bar;

    // Track positions of rover, habitat, and target
    Vector3 roverPos;
    Vector3 habitatPos;
    Vector3 targetPos;

    Vector3 oldPos;
    float totalDistance = 0;
    float convDistance = 0;

    int frames = 0;

    Stopwatch stopwatch = new Stopwatch();
    public Stopwatch phase_stopwatch = new Stopwatch();

    float power_available = 0.0f;
    float start_power_posx = 0.0f;
    float start_dist_posx = 0.0f;
    float dist_rem_posy = 0.0f;
    float power_posy = 0.0f;
    float power_rem_posy = 0.0f;
    float max_power_posx = -377.7f;
    float max_dist_posx = 301.8f;
    // found manually -- the amount of kwh used in one FixedUpdate at maximum torque
    float max_power_consumption;
    float max_speed = 30.0f;
    float max_travel_distance = 0.0f;

    //Mark's additions
    float rover_accel = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        targetPos = GameObject.Find("StopCollider").transform.position;
        speed = transform.Find("speed_text");
        // UTC = transform.Find("utc_text");
        timer = transform.Find("timer_text");
        PET_timer = transform.Find("PET_text");
        mp_text = transform.Find("mp_text");
        total_distance = transform.Find("total_distance_text");
        // dist_from_base = transform.Find("dist_from_base_text");
        dist_to_empty = transform.Find("dist_from_target_text");
        dist_to_empty_bar = transform.Find("distance_remaining_bar");
        angle = transform.Find("angle_text");
        // power = transform.Find("power_text");
        // avg_kmh = transform.Find("avg_kmh_text");
        // avg_range = transform.Find("avg_range_text");
        power_efficiency = transform.Find("power_efficiency_bar");
        power_efficiency_text = transform.Find("power_efficiency_text");

        power_remaining = transform.Find("power_remaining_bar");
        power_remaining_text = transform.Find("power_remaining_text");
        

        heading = transform.Find("heading");

        rover = GameObject.Find("rover");
        habitat = GameObject.Find("Habitat");
        rover_rb = rover.GetComponent<Rigidbody>();
        rover_script = rover.GetComponent<PowerController>();

        // Stopwatch setup
        stopwatch.Start();
        phase_stopwatch.Start();

        oldPos = rover.transform.position;

        start_power_posx = power_efficiency.GetComponent<RectTransform>().localPosition.x;
        start_dist_posx = dist_to_empty_bar.GetComponent<RectTransform>().localPosition.x;
        dist_rem_posy = dist_to_empty_bar.GetComponent<RectTransform>().localPosition.y;
        power_posy = power_efficiency.GetComponent<RectTransform>().localPosition.y;
        power_rem_posy = power_remaining.GetComponent<RectTransform>().localPosition.y;
        // 0.0009 found manually -- max power consumption in KW
        max_power_consumption = 0.0009695652f / (Time.fixedDeltaTime / 3600);
    }

    public void setTargetPower(float kwh)
    {
        power_available = kwh;
        max_travel_distance =  max_speed * power_available / max_power_consumption;
    }

    // Update is called once per frame
    void Update()
    {
        // Get speed in m/s -> change to km/h by multiplying by 3.6
        speed.GetComponent<Text>().text = Mathf.Round(rover_rb.velocity.magnitude * 3.6f).ToString();

        

        // Update UTC
        // UTC.GetComponent<Text>().text = DateTime.UtcNow.ToString("HH:mm");

        // Update stopwatch
        timer.GetComponent<Text>().text = stopwatch.Elapsed.ToString(@"mm\:ss");
        
        PET_timer.GetComponent<Text>().text = phase_stopwatch.Elapsed.ToString(@"mm\:ss");

        // Update power consumption
        // power.GetComponent<Text>().text = rover_script.kwh.ToString();

        // Update average kmh used per km
        // float kmh_calc = Mathf.Round(((convDistance*10/rover_script.kwh)))/10;
        // avg_kmh.GetComponent<Text>().text = (kmh_calc).ToString();

        // Provide estimate for avg amount of km left
        // avg_range.GetComponent<Text>().text = (rover_script.battery * kmh_calc).ToString();

        // Calculate total distances traveled and update corresponding texts
        // These only happen every 10 frames because they are expensive
        if (frames % 10 == 0)
        {
            // Calculate distance rover travels
            Vector3 distanceVector = rover.transform.position - oldPos;
            float distanceThisFrame = distanceVector.magnitude;
            totalDistance += distanceThisFrame;
            oldPos = rover.transform.position;
            convDistance = totalDistance/1000;
            total_distance.GetComponent<Text>().text = (Mathf.Round(convDistance*10)/10).ToString();

            // Calculate distance from rover to habitat
            // roverPos = rover.transform.position;
            // habitatPos = habitat.transform.position;
            // Vector3 rh_dist = roverPos - habitatPos;
            // float rh_mag = rh_dist.magnitude;
            // convDistance = rh_mag/1000;
            // dist_from_base.GetComponent<Text>().text = (Mathf.Round(convDistance*10)/10).ToString();

            // Calculate distance from rover to target
            // targetPos = target.transform.position;
            // Vector3 rt_dist = roverPos - targetPos;
            // float rt_mag = rt_dist.magnitude;
            // convDistance = rt_mag/1000;
            // dist_from_target.GetComponent<Text>().text = (Mathf.Round(convDistance*10)/10).ToString();

            //calculate direction from rover to target
            Vector3 targetPosLocal = rover.transform.InverseTransformPoint(targetPos);
            float targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.z) * Mathf.Rad2Deg;
            heading.eulerAngles = new Vector3(0, 0, targetAngle);


            // power usage
            float power_eff = rover_script.getPowerEfficiency();
            power_efficiency.GetComponent<RectTransform>().sizeDelta = new Vector2(150 * power_eff, 50);
            float new_posx = start_power_posx - (start_power_posx - max_power_posx) * power_eff;
            power_efficiency.GetComponent<RectTransform>().localPosition = new Vector3(new_posx, power_posy, 0);

            if (power_eff < 0.1)
            {
                power_efficiency.GetComponent<RawImage>().texture = red_progress_bar;
            }
            else
            {
                power_efficiency.GetComponent<RawImage>().texture = green_progress_bar;
            }

            power_efficiency_text.GetComponent<Text>().text = Mathf.RoundToInt(power_eff * 100).ToString();

            // power remaining
            int power_rem = Mathf.RoundToInt(100 * (power_available - (rover_script.kwh / power_available)));
            float power_left = Mathf.Clamp((power_available - rover_script.kwh), 0.1f, 100.0f);

            power_remaining_text.GetComponent<Text>().text = Math.Round(power_left, 1).ToString("N1");
            float pct_power_remaining = Mathf.Clamp(power_left / power_available, 0.01f, 1.0f);
            new_posx = start_power_posx - (start_power_posx - max_power_posx) * pct_power_remaining;
            power_remaining.GetComponent<RectTransform>().sizeDelta = new Vector2(150 * pct_power_remaining, 50);
            power_remaining.GetComponent<RectTransform>().localPosition = new Vector3(new_posx, power_rem_posy, 0);
            if (pct_power_remaining < 0.1)
            {
                power_remaining.GetComponent<RawImage>().texture = red_progress_bar;
            }
            else
            {
                power_remaining.GetComponent<RawImage>().texture = green_progress_bar;
            }


            // distance to empty
            // since we have to assume max speed for distance remaining, give a little bit extra power
            float power_left_extra = Mathf.Clamp((power_available*1.25f - rover_script.kwh), 0.1f, 100.0f);
            float dist_remaining = max_speed * power_left_extra / max_power_consumption; // (Km / h) * (Kwh) / (Kw) = Km
            dist_to_empty.GetComponent<Text>().text = Math.Round(dist_remaining,1).ToString("N1");
            float pct_max_distance = Mathf.Clamp(dist_remaining / max_travel_distance, 0.01f, 1.00f);
            
            dist_to_empty_bar.GetComponent<RectTransform>().sizeDelta = new Vector2(150 * pct_max_distance, 50);
            new_posx = start_dist_posx - (start_dist_posx - max_dist_posx) * pct_max_distance;
            dist_to_empty_bar.GetComponent<RectTransform>().localPosition = new Vector3(new_posx, dist_rem_posy, 0);
            if (pct_max_distance < 0.1)
            {
                dist_to_empty_bar.GetComponent<RawImage>().texture = red_progress_bar;
            }
            else
            {
                dist_to_empty_bar.GetComponent<RawImage>().texture = green_progress_bar;
            }

            // Calculate angle of inclination
            // Find global wheel positions
            Vector3 frontLeft = rover.transform.Find("wheels").transform.Find("FrontLeft").transform.position;
            Vector3 backLeft = rover.transform.Find("wheels").transform.Find("BackLeft").transform.position;
            Vector3 wheelDistance = frontLeft - backLeft;
            float magDist = wheelDistance.magnitude;
            float yDist = frontLeft.y - backLeft.y;
            float incline = Mathf.Sin(yDist / magDist);
            incline = incline * Mathf.Rad2Deg;
            //UnityEngine.Debug.Log("Angle: " + angle);
            angle.GetComponent<Text>().text = (Mathf.Round(incline*10)/10).ToString();

            frames = 0;
        }

        frames++;
    }
}
