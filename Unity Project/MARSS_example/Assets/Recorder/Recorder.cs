using System;
using System.IO;
using System.Text;
using Unity.Robotics.ROSTCPConnector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Recorder
{
    /// <summary>
    /// Add this component to a GameObject to Record Mic Input 
    /// </summary>
    [AddComponentMenu("AhmedSchrute/Recorder")]
    [RequireComponent(typeof(AudioSource))]
    public class Recorder : MonoBehaviour
    {
        #region Constants &  Static Variables
        /// <summary>
        /// Audio Source to store Microphone Input, An AudioSource Component is required by default
        /// </summary>
        static AudioSource audioSource;
        /// <summary>
        /// The samples are floats ranging from -1.0f to 1.0f, representing the data in the audio clip
        /// </summary>
        static float[] samplesData;
        /// <summary>
        /// WAV file header size
        /// </summary>
        const int HEADER_SIZE = 44;

        #endregion

        #region Editor Exposed Variables

        /// <summary>
        /// Set a keyboard key for saving the Audio File
        /// </summary>
        [Tooltip("Set a keyboard key for saving the Audio File")]
        public KeyCode saveKey,startKey;
        /// <summary>
        /// Set a Button to trigger writing the WAV file and Saving the Audio 
        /// </summary>
        public Button SaveButton;
        public Button StartButton;
        /// <summary>
        /// What should the saved file name be, the file will be saved in Streaming Assets Directory
        /// </summary>
        [Tooltip("What should the saved file name be, the file will be saved in Streaming Assets Directory, Don't add .wav at the end")]
        public static string fileName;
        public static int audio_index = 0;

        private RosConnectionExample rosConnection;
        private float startRecordingTime;

        #endregion

        #region MonoBehaviour Callbacks

        void Start()
        {
            rosConnection=GameObject.Find("ROS_center").GetComponent<RosConnectionExample>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(startKey))
            {
                StartRecording();
            }

            if (Input.GetKeyDown(saveKey))
            {
                Save();
            }
        }

        #endregion

        #region Recorder Functions

        public void StartRecording()
        {
            Debug.Log("Pressed start!!!!!!!!!!!!!!!!!");
            startRecordingTime = Time.time;
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 300, 44100);
        }

        public void Save()
        {

            
            while (!(Microphone.GetPosition(null) > 0)) { }
            Microphone.End(Microphone.devices[0]);
            AudioClip trimedClip = AudioClip.Create(audioSource.clip.name, (int)((Time.time - startRecordingTime) * audioSource.clip.frequency), audioSource.clip.channels, audioSource.clip.frequency, false);
            float[] data = new float[(int)((Time.time - startRecordingTime) * audioSource.clip.frequency)];
            audioSource.clip.GetData(data,0);
            trimedClip.SetData(data, 0);
            audioSource.clip = trimedClip;

            //samplesData = new float[audioSource.clip.samples * audioSource.clip.channels];
            //audioSource.clip.GetData(samplesData, 0);
 
#if UNITY_EDITOR
            string filePath = Path.Combine(Application.streamingAssetsPath, audio_index.ToString() + ".wav");
#else
            string filePath = Path.Combine(UnityEngine.Windows.Directory.localFolder, audio_index.ToString() + ".wav");
#endif
            // Delete the file if it exists.
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            try
            {
                WriteWAVFile(audioSource.clip, filePath);
                //filePath=Path.Combine(Application.streamingAssetsPath,  "1000.wav");
                byte[] bytes = ConvertWAVtoByteArray(filePath);
                rosConnection.publishBytes(bytes);
                Debug.Log($"File Saved Successfully at {filePath}");
                audio_index += 1;
            }
            catch (DirectoryNotFoundException)
            {
                Debug.Log($"target path: {filePath}");
                Debug.LogError("Please, Create a StreamingAssets Directory in the Assets Folder");
            }
           
        }

        public static byte[] ConvertWAVtoByteArray(string filePath)
        {
            //Open the stream and read it back.

            FileStream fs = File.OpenRead(filePath);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        // WAV file format from http://soundfile.sapp.org/doc/WaveFormat/
        static void WriteWAVFile(AudioClip clip, string filePath)
        {
            float[] clipData = new float[clip.samples];

            //Create the file.
            using (Stream fs = File.Create(filePath))
            {
                int frequency = clip.frequency;
                int numOfChannels = clip.channels;
                int samples = clip.samples;
                fs.Seek(0, SeekOrigin.Begin);

                //Header

                // Chunk ID
                byte[] riff = Encoding.ASCII.GetBytes("RIFF");
                fs.Write(riff, 0, 4);

                // ChunkSize
                byte[] chunkSize = BitConverter.GetBytes((HEADER_SIZE + clipData.Length) - 8);
                fs.Write(chunkSize, 0, 4);

                // Format
                byte[] wave = Encoding.ASCII.GetBytes("WAVE");
                fs.Write(wave, 0, 4);

                // Subchunk1ID
                byte[] fmt = Encoding.ASCII.GetBytes("fmt ");
                fs.Write(fmt, 0, 4);

                // Subchunk1Size
                byte[] subChunk1 = BitConverter.GetBytes(16);
                fs.Write(subChunk1, 0, 4);

                // AudioFormat
                byte[] audioFormat = BitConverter.GetBytes(1);
                fs.Write(audioFormat, 0, 2);

                // NumChannels
                byte[] numChannels = BitConverter.GetBytes(numOfChannels);
                fs.Write(numChannels, 0, 2);

                // SampleRate
                byte[] sampleRate = BitConverter.GetBytes(frequency);
                fs.Write(sampleRate, 0, 4);

                // ByteRate
                byte[] byteRate = BitConverter.GetBytes(frequency * numOfChannels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
                fs.Write(byteRate, 0, 4);

                // BlockAlign
                ushort blockAlign = (ushort)(numOfChannels * 2);
                fs.Write(BitConverter.GetBytes(blockAlign), 0, 2);

                // BitsPerSample
                ushort bps = 16;
                byte[] bitsPerSample = BitConverter.GetBytes(bps);
                fs.Write(bitsPerSample, 0, 2);

                // Subchunk2ID
                byte[] datastring = Encoding.ASCII.GetBytes("data");
                fs.Write(datastring, 0, 4);

                // Subchunk2Size
                byte[] subChunk2 = BitConverter.GetBytes(samples * numOfChannels * 2);
                fs.Write(subChunk2, 0, 4);

                // Data

                clip.GetData(clipData, 0);
                short[] intData = new short[clipData.Length];
                byte[] bytesData = new byte[clipData.Length * 2];

                int convertionFactor = 32767;

                for (int i = 0; i < clipData.Length; i++)
                {
                    intData[i] = (short)(clipData[i] * convertionFactor);
                    byte[] byteArr = new byte[2];
                    byteArr = BitConverter.GetBytes(intData[i]);
                    byteArr.CopyTo(bytesData, i * 2);
                }
                
                fs.Write(bytesData, 0, bytesData.Length);
            }
        }

#endregion
    }
}