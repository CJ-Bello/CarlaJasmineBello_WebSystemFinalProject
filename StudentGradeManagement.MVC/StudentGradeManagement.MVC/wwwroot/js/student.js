const dashboard = document.getElementById("studentDashboard");

const studentNumberElement = document.getElementById("studentNumber");
const studentFullNameElement = document.getElementById("studentFullName");
const studentNameElement = document.getElementById("studentName");

const gradesTableBody = document.getElementById("studentGradesTableBody");
const dashboardMessage = document.getElementById("studentDashboardMessage");

const token = dashboard.dataset.token;

const apiUrl = "https://localhost:7146/api/student/grades";



async function loadStudentGrades() {
    try {
        dashboardMessage.textContent = "Loading your grades...";
        dashboardMessage.className = "";

        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Accept": "application/json"
            }
        });

        if (!response.ok) {
            if (response.status === 401) {
                throw new Error("Your session has expired. Please log in again.");
            }

            if (response.status === 403) {
                throw new Error("You are not authorized to access this dashboard.");
            }

            throw new Error("Failed to retrieve your grades.");
        }

        const studentData = await response.json();

        studentNumberElement.textContent = studentData.studentNumber;
        studentFullNameElement.textContent = studentData.studentName;
        studentNameElement.textContent = studentData.studentName;

        const grades = studentData.grades;

        gradesTableBody.innerHTML = "";

        if (!grades || grades.length === 0) {
            dashboardMessage.textContent = "You currently have no grade records.";
            return;
        }

        grades.forEach(grade => {
            const row = document.createElement("tr");

            const values = [
                grade.subjectCode,
                grade.subjectName,
                grade.facultyName,
                grade.grade,
                grade.remarks,
                grade.semester,
                grade.schoolYear
            ];

            values.forEach(value => {
                const cell = document.createElement("td");

                cell.textContent = value ?? "";

                row.appendChild(cell);
            });

            gradesTableBody.appendChild(row);
        });

        dashboardMessage.textContent =
            `Showing ${grades.length} grade record(s).`;

    } catch (error) {
        dashboardMessage.textContent = error.message;
        dashboardMessage.className = "error";

        console.error("Student Dashboard Error:", error);
    }
}

loadStudentGrades();
