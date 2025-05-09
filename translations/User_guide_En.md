# EasySave Application User Guide (Console)

This guide will assist you in using the EasySave console application, designed to manage full or differential backup tasks.

## Before You Begin

1. **Open the Project:**  
    - Locate and open the `ConsolApp.csproj` file with Visual Studio. This file contains the entire project.

2. **Run the Code:**  
    - Once the project is open, click the "Start" button or press `F5` to launch the application.

---

## 1. Initial Choice

When the application starts, the following menu is displayed:

```
[1] Change Language  
[2] Start a Backup  
[3] Exit  
```

Enter `2` to start a backup.

---

## 2. Backup Menu

A second menu appears:

```
[1] Display Backup Tasks  
[2] Add a Backup  
[3] Delete a Backup  
[4] Execute a Backup  
[5] Exit  
```

---

## 3. Add a Backup

Enter `2` to add a new backup. Then proceed as follows:

1. **Enter a Backup Name**  
    - Example: `Name: > MyBackup`

2. **Choose a Backup Type**  
    ```
    [1] Full  
    [2] Differential  
    ```
    - Enter `1` or `2` depending on the desired type.

3. **Select the Source Drive**  
    - For example, enter `C` to access `C:\`.  
    - The drive's contents will then be displayed.  
      - Navigate through folders by entering their number.  
      - Confirm with `s`.  
      - Cancel with `q`.

4. **Then Select the Destination Drive**  
    - Repeat the same procedure to define the target folder.

---

## 4. Execute a Backup

Return to the previous menu and enter `4`. Then you will need to:

- Enter the name of the backup you created (e.g., `MyBackup`).  
- The backup will start automatically.  
- A confirmation or error message will be displayed depending on the result.

---

## 5. Display or Delete a Backup (Optional)

- Enter `1` to display the list of saved backups.  
- Enter `3` to delete a backup (you will need to enter its name).

---

## 6. Return or Exit the Application

- Enter `q` to return to the main menu.  
- In the main menu, enter `3` to exit the application.  
