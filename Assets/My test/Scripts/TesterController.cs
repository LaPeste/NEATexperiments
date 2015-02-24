using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public class TesterController : UnitController
{
    public float Speed = 4;
    private Vector3 _speed;
    bool IsRunning;
    IBlackBox box;
    float sensorRange = 5;
    GameObject target;
    public float radarRadius;
   // float initialDistance; //this is the initial distance between the target and the goal
    public float negativeReward = 200f;
    private int breakRuleCounter;


    void Awake()
    {
        breakRuleCounter = 0;
    }
     // Use this for initialization
	void Start ()
    {
        target = GameObject.Find("Goal");
  //      initialDistance = Mathf.Sqrt(Mathf.Pow(this.transform.position.x + target.transform.position.x, 2) + Mathf.Pow(this.transform.position.y + target.transform.position.y, 2));
	}
	
	// Update is called once per frame
	void Update ()
    {
	    if(IsRunning)
        {
            /*the idea is
             * 1- prepare inputs, based on the sensors that you want
             * 2- write input in a ISignalArray inArray
             * 3- activate your IBlackBox (called box in my case), so it elaborates the output
             * *** N.B. REMEMBER TO CHANGE THE AMOUNT OF INPUT AND OUTPUT WRITTEN IN THE DOCUMENT FOR THE CONFIGURATION OF THE NEURAL NETWORK ***
             * 4- write the output in a ISignalArray outArray
             * 5- decode the output as you need and apply it to the target
             */

            //from a practical perspective (not needed in theory) inputs for the neural network
            //need to be normalized, however it is not mandatory, but it makes the learning smoother
    
            float frontRadar = 0;
            float leftRadar = 0;
            float rightRadar = 0;
            float backRadar = 0;
            float canSee = 0;

            RaycastHit hit;

            int goalMask = LayerMask.GetMask("Goal");
            //Vector3 right = new Vector3(this.transform.forward.z, this.transform.forward.y, -this.transform.forward.x);
            //Vector3 backward = -this.transform.forward;
            //Vector3 left = -right;

/***** OLD APPROACH *****
            Debug.DrawRay(transform.position, this.transform.forward * 4, Color.cyan);
            if (Physics.SphereCast(this.transform.position, this.collider.bounds.size.x * 4, this.transform.forward, out hit, 100f, layerMask))
            {
                Debug.LogWarning("hit from front");
                frontRadar = 1;
            }

            Debug.DrawRay(transform.position, right * 4, Color.green);
            if (Physics.SphereCast(this.transform.position, this.collider.bounds.size.x * 4, right, out hit, 100f, layerMask))
            {
                Debug.LogWarning("hit from right");
                rightRadar = 1;
            }

            Debug.DrawRay(transform.position, left * 4, Color.blue);
            if (Physics.SphereCast(this.transform.position, this.collider.bounds.size.x * 4, left, out hit, 100f, layerMask))
            {
                Debug.LogWarning("hit from left");
                leftRadar = 1;
            }

            Debug.DrawRay(transform.position, backward * 4, Color.red);
            if (Physics.SphereCast(this.transform.position, this.collider.bounds.size.x * 4, backward, out hit, 100f, layerMask))
            {
                Debug.LogWarning("hit from back");
                backRadar = 1;
            }
*/
            Collider[] radarHit = Physics.OverlapSphere(this.transform.position, radarRadius, goalMask);
            //rotating forward vector -45°. This will be the beginning of the front emisphere of the radar
            //IMPORTANT: FOR THIS TO WORK THE Y AXES IN UNITY HAS TO BE Z, OTHERWISE THE CODE MUST BE CHANGED!
            float radarInitX = transform.forward.x * Mathf.Cos(-Mathf.PI / 4) - transform.forward.z * Mathf.Sin(-Mathf.PI / 4);
            float radarInitZ = transform.forward.z * Mathf.Cos(-Mathf.PI / 4) + transform.forward.x * Mathf.Sin(-Mathf.PI / 4);
            Vector3 radarStartingPos = new Vector3(radarInitX, transform.forward.y, radarInitZ);
            float thirdEmisphereX = transform.forward.x * Mathf.Cos(-Mathf.PI * 3 / 4) - transform.forward.z * Mathf.Sin(-Mathf.PI * 3 / 4);
            float thirdEmisphereZ = transform.forward.z * Mathf.Cos(-Mathf.PI * 3 / 4) + transform.forward.x * Mathf.Sin(-Mathf.PI * 3 / 4);
            Vector3 endThirdEmispherePos = new Vector3(thirdEmisphereX, transform.forward.y, thirdEmisphereZ);
            //Debug.DrawRay(transform.position, radarStartingPos * radarRadius, Color.green);
            //Debug.DrawRay(transform.position, endThirdEmispherePos * radarRadius, Color.green);
            //Debug.DrawRay(transform.position, transform.forward * radarRadius, Color.blue);

            for(int i = 0; i < radarHit.Length; i++)
            {
                //vector between the element in the radar and the tester robot
                Vector3 vecElemHit = new Vector3(radarHit[i].transform.position.x - transform.position.x, radarHit[i].transform.position.y - transform.position.y, radarHit[i].transform.position.z - transform.position.z);
                
                //Debug.DrawRay(transform.position, vecElemHit * radarRadius, Color.magenta);
                //angle between the beginning of the front emisphere of the radar and the vector from the robot tester to the object in the radar
                float temp = Vector3.Dot(radarStartingPos, vecElemHit) / (vecElemHit.magnitude * radarStartingPos.magnitude);
                //Debug.Log("Temp is " + temp);
                float angleStartToHit = Mathf.Acos(temp); //this is in Radiants

                float angleDegree = angleStartToHit * (180 / Mathf.PI); //conversion in degree
                //FUCKING UNITY, I HAD TO DROP THIS SOLUTION JUST BECAUSE IT SUCKS AND IT DOESN'T GIVE YOU ANGLES BIGGER THAN 180°....-.-
                //if ((Mathf.Cos(temp) < 0 && Mathf.Sin(temp) < 0) || (Mathf.Cos(angleStartToHit) > 0 && Mathf.Sin(angleStartToHit) < 0))
                //{
                //    Debug.LogError("over 180°");
                //    angleDegree = 360f - angleDegree;
                //}

                temp = Vector3.Dot(endThirdEmispherePos, vecElemHit) / (vecElemHit.magnitude * endThirdEmispherePos.magnitude);
                //Debug.Log("Temp is " + temp);
                float angleEndThirdEmisphere = Mathf.Acos(temp); //this is in Radiants
                float thirdEndDegree = angleEndThirdEmisphere * (180 / Mathf.PI); //conversion in degree
                if (thirdEndDegree < 90)
                    angleDegree = 360f - angleDegree;

                //Debug.Log("the angle from beginning " + angleDegree);
               // Debug.Log("the angle end first emisphere " + thirdEndDegree);
                if (angleDegree <= 90 && angleDegree >= 0) //front emisphere
                {
                    //Debug.LogWarning("hit from front");
                    frontRadar = 1;
                }
                else if (angleDegree > 90 && angleDegree <= 180) //left emisphere
                {
                    //Debug.LogWarning("hit from left");
                    leftRadar = 1;
                }
                else if (angleDegree > 180 && angleDegree <= 270) //rear emispehere
                {
                   // Debug.LogWarning("hit from rear");
                    backRadar = 1;
                }
                else if (angleDegree > 270 && angleDegree <= 360) // right emisphere
                {
                    //Debug.LogWarning("hit from right");
                    rightRadar = 1;
                }

                int ruleMask = LayerMask.GetMask("Rule"); 
                if(Physics.Raycast(transform.position, vecElemHit, radarRadius, ruleMask))
                {
                    Debug.LogWarning("rule found between the bot and the goal");
                    canSee = 1;
                }
                else
                    canSee = 0;

            }

            ISignalArray inputArray = box.InputSignalArray;
            inputArray[0] = frontRadar;
            inputArray[1] = leftRadar;
            inputArray[2] = rightRadar;
            inputArray[3] = backRadar;
            inputArray[4] = canSee;

            box.Activate();

            ISignalArray outputArray = box.OutputSignalArray;

            //output decode and application
            float max = -2; //a random small number
            int neuronPos = -1;
            for (int i = 0; i < 5; i++ )
            {
                if ((float)outputArray[i] > max)
                {
                    max = (float)outputArray[i];
                    neuronPos = i;
                }

            }

            Vector3 moveDist = Vector3.zero;

            /***** DEBUG PURPOUSES ***/
            //neuronPos = 4;
            /***** DEBUG PURPOUSES ***/


            //the maximum output is going to be the direction
            switch(neuronPos)
            {
                case 0: //Debug.Log("the direction is forward"); //forward
                    moveDist = Speed * transform.forward * Time.deltaTime;
                    break;
                case 1: transform.Rotate(new Vector3(0, -45, 0));//left
                    //Debug.Log("the direction is left");
                    break;
                case 2: transform.Rotate(new Vector3(0, 45, 0));//right
                    //Debug.Log("the direction is right");
                    break;
                case 3: transform.Rotate(new Vector3(0, 90, 0)); //backward
                    //Debug.Log("the direction is backward");
                    break;
                case 4: moveDist = Vector3.zero;
                    //Debug.Log("time to stop, you're on the target!");
                    break;
                default: Debug.LogError("Impossible to select the right output");
                    break;
            }
            rigidbody.MovePosition(rigidbody.position + moveDist);
            //Debug.Log("the var go = " + go);
            //Debug.Log("the var turn = " + turn);
            

     /*       float turnAngle;
            if (turn < 0) //left
                turnAngle = -45;
            else if (turn > 0) //right
                turnAngle =     45;
            else
                turnAngle = 0; //don't turn
    */
        //    transform.Rotate(new Vector3(0, turnAngle, 0));
            
        }
	}

    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.IsRunning = true;
    }

    public override void Stop()
    {
        this.IsRunning = false;
    }

    //this is always called only one time at the end of the generation time for performing
    public override float GetFitness()
    {
        //hyperbolic function
        float actualPosition = Mathf.Sqrt(Mathf.Pow(this.transform.position.x - target.transform.position.x, 2) + Mathf.Pow(this.transform.position.y - target.transform.position.y, 2));
        float normPos = (actualPosition - 1f) / (35f - 1f); //I thought that normilizing between 0 and 1 could could give better results, since the plot of the hyperbolic function
        //Debug.LogError("normPos is " + normPos);
        if (normPos < 0) //kinda of no sense
            normPos = 0.1f;
        if (actualPosition < 0) //kinda of no sense
            actualPosition = 0.1f;
        float result = 1 / normPos - breakRuleCounter * negativeReward;
        //Debug.Log("the fitness is = " + result);
        if (result < 0) //because in NEAT the fitness cannot be negative, never!!!
            result = 0;

        breakRuleCounter = 0;
        return result;
        //return 1 / normPos;

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Rule"))
        {
            breakRuleCounter++;
            Debug.LogError("rule broken " + breakRuleCounter + " times.");
        }
    }
}
