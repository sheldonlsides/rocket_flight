using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class move : MonoBehaviour
{
    Rigidbody rigidbody = null;
    AudioSource audio = null;

    [SerializeField]
    AudioClip mainEngine = null;

    [SerializeField]
    AudioClip dying = null;

    [SerializeField] AudioClip levelBegin = null;
    [SerializeField] float rotationThrust = 100f;
    [SerializeField] float RocketThrust = 125f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] ParticleSystem mainEngineParticles = null;
    [SerializeField] ParticleSystem dyingParticles = null;
    [SerializeField] ParticleSystem successParticles = null;

    enum State {  Alive, Dying, Transcending }
    State state = State.Alive;
    bool collisionsEnabled = true;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        audio = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive)
        {
            return;
        }

        switch(collision.gameObject.tag)
        {
            case "friendly":
                print("You are still alive");
                break;
            case "enemy":
                if (collisionsEnabled)
                {
                    state = State.Dying;
                    audio.Stop();

                    audio.PlayOneShot(dying);

                    if (!dyingParticles.isPlaying)
                    {
                        dyingParticles.Play();
                    }

                    print("You are dead");
                    Invoke("reloadLevel", levelLoadDelay);
                }
                
                break;
            case "finish":
                successParticles.Play();

                state = State.Transcending;
                print("You finished");
                audio.PlayOneShot(levelBegin);
                Invoke("loadNextLevel", levelLoadDelay);
                break;
            case "lauchpad":
                print("You landed on lauchpad");
                break;
            default:
                state = State.Dying;
                print("You landed on the ground");
                break;

        }
    }

    private void ResponseToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (Debug.isDebugBuild)
            {
                loadNextLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsEnabled = !collisionsEnabled;
        }
    }

    private void loadNextLevel()
    {
        if (SceneManager.sceneCountInBuildSettings >= (SceneManager.GetActiveScene().buildIndex + 1))
        {
            SceneManager.LoadScene(0);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void reloadLevel()
    {
        print($"Current scnese is {SceneManager.GetActiveScene()}");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Update is called once per frame
    void Update()
    {
        ResponseToDebugKeys();

        if (state == State.Alive)
        {
            Audio();

            Thrust();

            Rotate();
        }
    }

    private void Audio()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!audio.isPlaying)
            {
                audio.PlayOneShot(mainEngine);
            }
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            audio.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            rigidbody.AddRelativeForce(Vector3.up *  Time.deltaTime * RocketThrust);

            if (!mainEngineParticles.isPlaying)
            {
                mainEngineParticles.Play();
            }
        }
    }

    private void Rotate()
    {
        var rotationSpeed = 0f;
        
        rigidbody.angularVelocity = Vector3.zero; //removes rotation because of physics
        rotationSpeed = rotationThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.back * rotationSpeed);
        }

    }

}
