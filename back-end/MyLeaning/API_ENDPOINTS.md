# REST API Endpoints

## Base URL
```
https://api.japaneselearning.com/api
```

## Authentication
All endpoints except Auth endpoints require:
```
Authorization: Bearer {access_token}
```

---

## 1. Authentication Endpoints

### Register
```http
POST /auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass@123",
  "fullName": "John Doe"
}

Response 201:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64_encoded_token",
  "expiresIn": 900,
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "john_doe",
  "email": "john@example.com"
}
```

### Login
```http
POST /auth/login
Content-Type: application/json

{
  "emailOrUsername": "john_doe",
  "password": "SecurePass@123"
}

Response 200:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64_encoded_token",
  "expiresIn": 900,
  "userId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### Refresh Token
```http
POST /auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "base64_encoded_token"
}

Response 200:
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "new_base64_token",
  "expiresIn": 900
}
```

### Logout
```http
POST /auth/logout
Authorization: Bearer {accessToken}

Response 200:
{
  "message": "Logged out successfully"
}
```

---

## 2. Course Management Endpoints

### Get All Courses
```http
GET /courses?level=N5&page=1&pageSize=10&search=beginner
Authorization: Bearer {accessToken}

Response 200:
{
  "data": [
    {
      "id": "course-id-1",
      "title": "Japanese Basics",
      "description": "Learn basic Japanese",
      "level": "N5",
      "durationHours": 20,
      "imageUrl": "https://...",
      "lessonsCount": 10,
      "userProgress": 45.5
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 25,
    "totalPages": 3
  }
}
```

### Get Course Details
```http
GET /courses/{courseId}
Authorization: Bearer {accessToken}

Response 200:
{
  "id": "course-id-1",
  "title": "Japanese Basics",
  "description": "Learn basic Japanese",
  "level": "N5",
  "durationHours": 20,
  "imageUrl": "https://...",
  "lessons": [
    {
      "id": "lesson-id-1",
      "lessonNumber": 1,
      "title": "Hiragana",
      "description": "Learn hiragana characters",
      "durationMinutes": 45,
      "progress": 100
    }
  ],
  "userProgress": {
    "percentage": 45.5,
    "timeSpentHours": 9.2,
    "lastAccessDate": "2024-01-15T10:30:00Z"
  }
}
```

### Create Course (Admin Only)
```http
POST /courses
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "title": "Advanced Japanese",
  "description": "Advanced level Japanese",
  "level": "N2",
  "durationHours": 40,
  "imageUrl": "https://..."
}

Response 201:
{
  "id": "new-course-id",
  "title": "Advanced Japanese",
  ...
}
```

---

## 3. Lesson Endpoints

### Get Lessons in Course
```http
GET /courses/{courseId}/lessons
Authorization: Bearer {accessToken}

Response 200:
{
  "data": [
    {
      "id": "lesson-id-1",
      "courseId": "course-id-1",
      "lessonNumber": 1,
      "title": "Hiragana",
      "description": "Learn hiragana characters",
      "durationMinutes": 45,
      "topicsCount": 3,
      "progress": 100
    }
  ]
}
```

### Get Lesson Details
```http
GET /lessons/{lessonId}
Authorization: Bearer {accessToken}

Response 200:
{
  "id": "lesson-id-1",
  "courseId": "course-id-1",
  "lessonNumber": 1,
  "title": "Hiragana",
  "description": "Learn hiragana characters",
  "durationMinutes": 45,
  "topics": [
    {
      "id": "topic-id-1",
      "title": "Basic Hiragana",
      "type": "Vocabulary",
      "itemsCount": 46,
      "progress": 100
    }
  ],
  "userProgress": 100
}
```

---

## 4. Topic & Content Endpoints

### Get Topic Content
```http
GET /lessons/{lessonId}/topics/{topicId}
Authorization: Bearer {accessToken}

Response 200:
{
  "id": "topic-id-1",
  "lessonId": "lesson-id-1",
  "title": "Basic Hiragana",
  "type": "Vocabulary",
  "description": "Learn 46 basic hiragana characters",
  "items": [
    {
      "id": "vocab-id-1",
      "type": "vocabulary",
      "kanjiForm": "?",
      "hiraganaForm": "?",
      "romaji": "a",
      "meaning": "The letter 'a'",
      "exampleSentences": ["?????"]
    }
  ]
}
```

### Get Vocabulary Item
```http
GET /vocabulary/{vocabularyId}
Authorization: Bearer {accessToken}

Response 200:
{
  "id": "vocab-id-1",
  "kanjiForm": "?",
  "hiraganaForm": "??",
  "katakanaForm": "??",
  "romaji": "hon",
  "meaning": "book",
  "partOfSpeech": "noun",
  "audioUrl": "https://...",
  "imageUrl": "https://...",
  "exampleSentences": [
    "???? (hon wo yomu) - to read a book"
  ]
}
```

---

## 5. Exercise Endpoints

### Get Exercises for Topic
```http
GET /topics/{topicId}/exercises?difficulty=Medium
Authorization: Bearer {accessToken}

Response 200:
{
  "data": [
    {
      "id": "exercise-id-1",
      "title": "Hiragana Recognition",
      "type": "MultipleChoice",
      "difficulty": "Medium",
      "questionsCount": 10,
      "timeLimit": 15,
      "userHighScore": 95
    }
  ]
}
```

### Start Exercise
```http
POST /exercises/{exerciseId}/start
Authorization: Bearer {accessToken}

Response 201:
{
  "sessionId": "session-id-1",
  "exerciseId": "exercise-id-1",
  "questions": [
    {
      "id": "question-id-1",
      "type": "MultipleChoice",
      "questionText": "Which hiragana is this: ?",
      "imageUrl": "https://...",
      "options": [
        {
          "id": "option-id-1",
          "text": "a"
        },
        {
          "id": "option-id-2",
          "text": "i"
        },
        {
          "id": "option-id-3",
          "text": "u"
        }
      ],
      "points": 10
    }
  ],
  "startTime": "2024-01-15T10:30:00Z",
  "timeLimit": 15
}
```

### Submit Answer
```http
POST /sessions/{sessionId}/answers
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "questionId": "question-id-1",
  "userResponse": "option-id-1",
  "timeSpentSeconds": 5
}

Response 200:
{
  "answerId": "answer-id-1",
  "isCorrect": true,
  "points": 10,
  "explanation": "Correct! This is the hiragana 'a'",
  "nextQuestionId": "question-id-2"
}
```

### Complete Exercise
```http
POST /sessions/{sessionId}/complete
Authorization: Bearer {accessToken}

Response 200:
{
  "sessionId": "session-id-1",
  "status": "Completed",
  "totalPoints": 95,
  "percentageScore": 95.0,
  "totalTimeSeconds": 850,
  "correctAnswers": 10,
  "totalQuestions": 10,
  "wrongAnswers": [],
  "completedAt": "2024-01-15T10:45:00Z"
}
```

### Get Exercise Stats
```http
GET /exercises/{exerciseId}/stats
Authorization: Bearer {accessToken}

Response 200:
{
  "exerciseId": "exercise-id-1",
  "attemptCount": 5,
  "highScore": 100,
  "averageScore": 88.5,
  "totalTimeSeconds": 4250,
  "bestAttempt": {
    "score": 100,
    "date": "2024-01-15T10:30:00Z"
  },
  "attempts": [
    {
      "sessionId": "session-id-1",
      "score": 95,
      "date": "2024-01-15T10:30:00Z"
    }
  ]
}
```

---

## 6. SRS (Spaced Repetition) Endpoints

### Get SRS Schedule
```http
GET /srs/schedule
Authorization: Bearer {accessToken}

Response 200:
{
  "dueToday": 15,
  "dueThisWeek": 42,
  "newCards": 5,
  "totalCards": 256,
  "nextReviewTime": "2024-01-15T14:30:00Z",
  "reviewsCompletedToday": 20,
  "reviewsCompletedThisWeek": 145,
  "statistics": {
    "masteredCards": 120,
    "reviewingCards": 100,
    "learningCards": 36
  }
}
```

### Get Due Cards
```http
GET /srs/cards/due?limit=10
Authorization: Bearer {accessToken}

Response 200:
{
  "data": [
    {
      "cardId": "srs-card-id-1",
      "vocabularyId": "vocab-id-1",
      "kanjiForm": "?",
      "meaning": "book",
      "srsLevel": 3,
      "nextReviewDate": "2024-01-15T14:30:00Z",
      "lastReviewDate": "2024-01-14T10:30:00Z",
      "reviewCount": 5,
      "lastQuality": 4
    }
  ],
  "dueCount": 15
}
```

### Review Card
```http
POST /srs/cards/{cardId}/review
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "quality": 4  // 0-5: 0=Fail, 3+=Pass, 5=Perfect
}

Response 200:
{
  "cardId": "srs-card-id-1",
  "previousLevel": 2,
  "newLevel": 3,
  "nextReviewDate": "2024-01-18T10:30:00Z",
  "easeFactor": 2.35,
  "interval": 7
}
```

### Add Vocabulary to SRS
```http
POST /srs/cards
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "vocabularyId": "vocab-id-1"
}

Response 201:
{
  "cardId": "srs-card-id-1",
  "vocabularyId": "vocab-id-1",
  "srsLevel": 1,
  "nextReviewDate": "2024-01-16T10:30:00Z"
}
```

### Get Card History
```http
GET /srs/cards/{cardId}/history
Authorization: Bearer {accessToken}

Response 200:
{
  "cardId": "srs-card-id-1",
  "reviews": [
    {
      "reviewId": "review-id-1",
      "reviewDate": "2024-01-14T10:30:00Z",
      "quality": 4,
      "previousLevel": 1,
      "newLevel": 2,
      "durationSeconds": 8
    }
  ],
  "statistics": {
    "totalReviews": 5,
    "correctReviews": 4,
    "accuracy": 80
  }
}
```

---

## 7. Progress Tracking Endpoints

### Get User Progress for Course
```http
GET /progress/courses/{courseId}
Authorization: Bearer {accessToken}

Response 200:
{
  "courseId": "course-id-1",
  "courseName": "Japanese Basics",
  "overallPercentage": 45.5,
  "totalTimeSpentHours": 9.2,
  "startDate": "2024-01-01T00:00:00Z",
  "lastAccessDate": "2024-01-15T10:30:00Z",
  "lessons": [
    {
      "lessonId": "lesson-id-1",
      "lessonNumber": 1,
      "title": "Hiragana",
      "percentage": 100,
      "topicsCompleted": 3,
      "topicsTotal": 3,
      "exercisesCompleted": 5,
      "timeSpentMinutes": 180
    }
  ]
}
```

### Get User Statistics
```http
GET /progress/statistics
Authorization: Bearer {accessToken}

Response 200:
{
  "totalExercisesCompleted": 125,
  "totalCorrectAnswers": 1050,
  "averageScore": 84.5,
  "totalStudyTimeHours": 45.5,
  "currentStreak": 12,
  "longestStreak": 28,
  "lastStudyDate": "2024-01-15T10:30:00Z",
  "totalPoints": 5250,
  "correctPercentage": 84.0,
  "thisWeekStats": {
    "exercisesCompleted": 18,
    "timeSpentHours": 6.5,
    "averageScore": 87.2
  },
  "thisMonthStats": {
    "exercisesCompleted": 42,
    "timeSpentHours": 18.3,
    "averageScore": 85.8
  }
}
```

### Get Daily Statistics
```http
GET /progress/daily-stats?days=30
Authorization: Bearer {accessToken}

Response 200:
{
  "data": [
    {
      "date": "2024-01-15",
      "exercisesCompleted": 5,
      "averageScore": 88.0,
      "timeSpentMinutes": 45,
      "correct": 42,
      "total": 48
    }
  ]
}
```

### Get Streak
```http
GET /progress/streak
Authorization: Bearer {accessToken}

Response 200:
{
  "currentStreak": 12,
  "longestStreak": 28,
  "streakDates": ["2024-01-15", "2024-01-14", "2024-01-13"],
  "nextBreakDate": "2024-01-16"
}
```

---

## 8. User Profile Endpoints

### Get Current User
```http
GET /users/me
Authorization: Bearer {accessToken}

Response 200:
{
  "id": "user-id-1",
  "username": "john_doe",
  "email": "john@example.com",
  "fullName": "John Doe",
  "avatarUrl": "https://...",
  "preferredLevel": "N5",
  "subscriptionPlan": "Premium",
  "subscriptionExpiryDate": "2024-12-31",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Update User Profile
```http
PUT /users/me
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "fullName": "John Updated",
  "bio": "Learning Japanese",
  "avatarUrl": "https://...",
  "preferredLevel": "N4"
}

Response 200:
{
  "id": "user-id-1",
  "username": "john_doe",
  ...
}
```

### Change Password
```http
POST /users/me/change-password
Authorization: Bearer {accessToken}
Content-Type: application/json

{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@456"
}

Response 200:
{
  "message": "Password changed successfully"
}
```

---

## Error Responses

### 400 Bad Request
```json
{
  "statusCode": 400,
  "message": "Invalid input",
  "errors": {
    "email": ["Email is invalid"],
    "password": ["Password must be at least 8 characters"]
  }
}
```

### 401 Unauthorized
```json
{
  "statusCode": 401,
  "message": "Unauthorized. Please provide a valid token."
}
```

### 403 Forbidden
```json
{
  "statusCode": 403,
  "message": "You don't have permission to access this resource"
}
```

### 404 Not Found
```json
{
  "statusCode": 404,
  "message": "Resource not found",
  "resourceId": "course-id-123"
}
```

### 409 Conflict
```json
{
  "statusCode": 409,
  "message": "Email already registered"
}
```

### 500 Internal Server Error
```json
{
  "statusCode": 500,
  "message": "Internal server error",
  "traceId": "0HMS1M3H9F7G7:00000001"
}
```

---

## Rate Limiting

All endpoints are rate limited:

```
Headers:
X-RateLimit-Limit: 100          # Requests per hour
X-RateLimit-Remaining: 87       # Requests remaining
X-RateLimit-Reset: 1705349200   # Unix timestamp when limit resets
```

---

## Pagination

All list endpoints support pagination:

```http
?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc

Response:
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 125,
    "totalPages": 13
  }
}
```

---

## Filtering & Search

Exercise endpoints support filtering:

```http
GET /exercises?difficulty=Easy&type=MultipleChoice&search=hiragana

Query Parameters:
- difficulty: Easy, Medium, Hard
- type: MultipleChoice, FillBlank, Listening, Mixed
- search: keyword search in exercise title/description
```

Course endpoints support filtering:

```http
GET /courses?level=N5&search=beginner&minDuration=10&maxDuration=50
```
