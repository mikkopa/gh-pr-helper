# GitHub Classroom assingment PR comment helper

This tool is used to provide comments in bulk to GHC assingment answers from a csv file.
The tool matches the answer repo with the GH username in the csv file and creates a PR comment from the comment text in the csv file.

## Requirements

1. A GitHub Classroom assignment with feedback option (Providing feedback for an assignment)[https://docs.github.com/en/education/manage-coursework-with-github-classroom/teach-with-github-classroom/create-an-individual-assignment#providing-feedback-for-an-assignment]

2. A GH CLI installed https://cli.github.com/

3. A csv file with columns for:
   1.  student's answer repo folder
   2.  a comment text

The tool assumes that when it is ran the user (the teacher) has logged in to GH.
Use `gh auth login` command or create a token and set it to environment variables.

## External libraries used

CsvHelper https://www.nuget.org/packages/CsvHelper/
CommandLineParser https://www.nuget.org/packages/CommandLineParser/ 


## Links

To login to GH
- (gh auth login)[https://cli.github.com/manual/gh_auth_login]
- (Creating a personal access token)[https://docs.github.com/en/github/authenticating-to-github/keeping-your-account-and-data-secure/creating-a-personal-access-token]

