# Japanese Learning Application - System Design Document

## Part 1: Database Schema & Domain Analysis

### ERD Description

```
The system is organized into 4 main schemas:

1. USER MANAGEMENT
   - Users: Core user information
   - UserProfiles: Extended profile data
   - Subscriptions: Plan management
   - UserStatistics: Aggregated learning stats

2. CONTENT MANAGEMENT
   - Courses > Lessons > Topics > Content Items
   - VocabularyItems: Japanese words with pronunciation
   - GrammarRules: Grammar explanations and examples
   - KanjiCharacters: Kanji with readings and meanings

3. LEARNING & EXERCISES
   - Exercises: Collections of questions
   - Questions: Individual exercise items
   - QuestionOptions: MCQ choices
   - UserSessions: Exercise attempts
   - UserAnswers: Individual question responses

4. PROGRESS TRACKING
   - UserProgress: Course progress per user
   - LessonProgress: Lesson-level granularity
   - UserStatistics: Aggregated metrics

5. SPACED REPETITION (SRS)
   - SrsCards: Individual flashcards
   - SrsReviews: Review history
   - SrsSchedules: Review scheduling

### Key Design Decisions

1. **Aggregate Root Pattern**
   - Course, Lesson, Topic are separate aggregates
   - Enables independent loading and modifications
   - Better for scalability

2. **Event Sourcing Ready**
   - UserAnswer contains all necessary data for audit trail
   - SrsReview captures complete state changes
   - Easy to implement event sourcing later

3. **Denormalization for Performance**
   - UserStatistics table: avoids constant aggregation
   - LessonProgress: cached progress data
   - SrsSchedule: pre-computed review dates

4. **Temporal Queries**
   - NextReviewDate: enables efficient SRS queries
   - LastAccessDate: tracks user activity
   - Timestamps on all entities

5. **High Concurrency Support**
   - Row versioning with UpdatedAt
   - Optimistic locking for critical operations
   - Partition-friendly schema
