using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.SpeakerRecognition;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract;
using Microsoft.ProjectOxford.SpeakerRecognition.Contract.Identification;

namespace ProxiCall_App.Services.SpeakerRecognition
{
    public class SRIdentification
    {
        private static SpeakerIdentificationServiceClient _serviceClient;

        //Profiles
        public static async Task CreateProfileAsync()
        {
            UpdateServiceClient();
            CreateProfileResponse creationResponse = await _serviceClient.CreateProfileAsync("en-us");
            Profile profile = await _serviceClient.GetProfileAsync(creationResponse.ProfileId);
        }

        public static async Task<Profile[]> RecoverProfilesListAsync()
        {
            UpdateServiceClient();

            Profile[] allProfiles = null;
            try
            {
                allProfiles = await _serviceClient.GetProfilesAsync();
            }
            catch (GetProfileException ex)
            {
                //window.Log("Error Retrieving Profiles: " + ex.Message);
                Console.WriteLine("Error Retrieving Profiles: " + ex.Message);
            }
            catch (Exception ex)
            {
                //window.Log("Error: " + ex.Message);
                Console.WriteLine("Error: " + ex.Message);
            }
            return allProfiles;
        }


        //Enroll
        public static async Task AddEnrollmentToProfile(Profile selectedProfile, string selectedFile, bool shortAudio)
        {
            OperationLocation processPollingLocation;
            using (Stream audioStream = File.OpenRead(selectedFile))
            {
                processPollingLocation = await _serviceClient.EnrollAsync(audioStream, selectedProfile.ProfileId, shortAudio);
            }

            EnrollmentOperation enrollmentResult;
            int numOfRetries = 10;
            TimeSpan timeBetweenRetries = TimeSpan.FromSeconds(5.0);
            while (numOfRetries > 0)
            {
                await Task.Delay(timeBetweenRetries);
                enrollmentResult = await _serviceClient.CheckEnrollmentStatusAsync(processPollingLocation);

                if (enrollmentResult.Status == Status.Succeeded)
                {
                    break;
                }
                else if (enrollmentResult.Status == Status.Failed)
                {
                    throw new EnrollmentException(enrollmentResult.Message);
                }
                numOfRetries--;
            }
            if (numOfRetries <= 0)
            {
                throw new EnrollmentException("Enrollment operation timeout.");
            }
        }

        //Identification
        //Ajouter ici méthode pour ID (cf sample)

        //Service Client
        private static void UpdateServiceClient()
        {
            _serviceClient = new SpeakerIdentificationServiceClient(Environment.GetEnvironmentVariable("SRApiKey"));
        }
    }
}
