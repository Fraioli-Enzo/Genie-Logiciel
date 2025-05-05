### **Software Specification - EasySave Version 1.0**

---

#### **1. Purpose of the Software**
EasySave 1.0 is a console application developed in .NET Core. Its goal is to allow the management and execution of backup jobs (full or differential) in a simple and effective way, while ensuring compatibility for both English and French-speaking users.

---

#### **2. Key Features**

##### **2.1 Management of Backup Jobs**
- Ability to create **up to 5 backup jobs**.
- Each job is defined by:
  - **Job name**
  - **Source directory**
  - **Target directory**
  - **Type of backup**:
    - Full backup
    - Differential backup
- Supports directories located on:
  - Local drives
  - External drives
  - Network drives (UNC paths)
- All files and subfolders from the source directory must be included.

##### **2.2 Execution of Jobs**
- On-demand execution of one or more backup jobs.
- Sequential execution of all configured jobs.
- Command-line usage for automated execution:
  - **Example 1**: `1-3` to execute backups 1 through 3.
  - **Example 2**: `1;3` to execute backups 1 and 3.

---

#### **3. Logging and Real-Time Tracking**

##### **3.1 Daily Log File**
- Real-time writing of actions to a daily log file in JSON format.
- Minimum data to log:
  - **Timestamp**
  - **Backup name**
  - **Full path of the source file (UNC)**
  - **Full path of the destination file**
  - **File size**
  - **Transfer time** in milliseconds (negative if error)
- Must be readable with Notepad, with line breaks between JSON elements.
- Logging should be implemented as a **Dynamic Link Library (DLL)** reusable in future projects, and must remain compatible with version 1.0.

##### **3.2 Real-Time Status File**
- A single file (`state.json`) maintains the live status of all backup jobs.
- Minimum data per job:
  - **Job name**
  - **Timestamp** of last action
  - **Status** (Active/Inactive)
  - If active:
    - **Total number of files**
    - **Total file size**
    - **Progress**:
      - Files remaining
      - Size remaining
    - **Current source file path**
    - **Current destination file path**

---

#### **4. Technical Constraints**

##### **4.1 File Location**
- Temporary paths such as `C:\temp\` are forbidden.
- All paths (log, status, configuration) must be server-compatible and accessible.

##### **4.2 File Format**
- JSON format required for all files (log, status, configuration).
- Line breaks for readability.
- **Pagination** is desirable for easier reading of large JSON files.

##### **4.3 Internationalization**
- The application must support at least **English and French**.

##### **4.4 Modularity**
- Logging must be implemented as a **DLL**.
- Future changes must maintain compatibility with version 1.0.

---

#### **5. Future Developments**
If version 1.0 is satisfactory, a **version 2.0** with a graphical interface (based on MVVM architecture) will be developed to provide a better user experience while preserving the strong foundations of version 1.0.

---

### **Conclusion**
EasySave 1.0 aims to deliver a simple, flexible, and extensible backup solution. Its real-time tracking, multi-language support, and robust logging system offer a solid foundation for the development of more advanced versions in the future.
