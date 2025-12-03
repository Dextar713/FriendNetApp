# Find your buddy

### Product Vision
FOR adults of all ages WHO want to meet new people or wish to connect two or more friends anonymously based on similar interests, THE "My Next Buddy" app is a social media product THAT allows people to meet other individuals who share the same activities, values, and ideas through a mutual acquaintance.

UNLIKE traditional social media apps that are bombarded with artificial content and exclusively promote the idea of perfection, social validation and superficial interactions, OUR PRODUCT is focused on (but not limited to) connecting people through common friends, or even randomly, based on profile, age, and location, further encouraging communication and helping extend social circles. We want to place an emphasis on today's social difficulties and provide certified support for these issues.

With this in mind, our main targets are both the younger generations who haven't had the chance to connect with people of their kind, and elders who are in need of someone to talk to and share life experiences with.

### Product Features

#### Matching

This is the base functionality of the app and the starting point of our idea: the ability to connect with people you never met and chat with them about common grounds. This feature is implemented in two ways: friends matching and random matching. *Friends matching* represents the ability to match anonymously 2 friends of you (that are not friends with each other); this operation creates a special chat where only these 2 chosen people can chat intimately (for a period of time while they can share some contacts like Instagram or WhatsApp) and become friends on this app, too. *Random matching* is a functionality that allows you to match randomly with people that you share multiple things in common, under certain filters like location, age, gender etc.; this feature is available only one time per day so its importance is not lost in the large palette of matches. One more functionality around matching is the ability to rate the anonymous friend that matched you based on the quality of the interaction - lower rating attracts lower opportunities for matching others. An equivalent for this on random matching would be rating the AI algorithm - this makes your future matches more valuable.

#### Profiles

For our random matching algorithms to work and also to have a starting point for the chatting functionalities, every user must setup a profile with various information: from basic information like name and address to very personal stuff like movies watched, books read and personal best in 5km run. Most of these information can be hidden to all the people (and only available for the algorithm), hidden only for non-friends or accessible by anyone. We will decide on the exact information we will add to profile after we research more on how this can be processed easiest by the AI model we plan to integrate for the matching functionality. Regarding profile customisation, we won't integrate profile pictures since we believe this app must be oriented around pure interactions.

#### Feed

We also intend to implement a feed tailored for each user based on the information from the profile consisting of friends posts, profile updates from friends and possibly some content from the random matching profiles which are not friends. We don't want to work a lot on this feature since keeping the users in our app doesn't match with our app's unwritten principles.

#### Chatting

After a match has been created by the system, a chat is also created instantly just for you two. Because we want to build this app only for connecting people and not long-term chatting or sharing different content, we don't offer support for fancy formatting or persistent messaging. Besides this, on every chat you have the possibility to rate the person you chat with and report it if needed.


### User Scenarios

Below are example user scenarios derived from the product vision and features.

1) New user signs up and creates profile

- Actors: New user
- Context: First-time app user.
- Main flow:
  1. Sign up with email/phone.
  2. Complete profile with age, location, interests.
  3. Set visibility for each field (Public, Friends-only, Private - algorithm only).
  4. Complete onboarding.
- Alternatives:
  - Skip optional fields.
  - Uploading a profile picture is not available.

2) Friend matching — anonymous introduction

- Actors: Inviter (user A), Friend 1, Friend 2
- Context: A wants to introduce two friends who aren't connected.
- Main flow:
  1. A selects two friends for anonymous matching.
  2. System creates an anonymous two-person chat for the matched friends.
  3. Friends join and chat for a limited time; may exchange permitted contact info.
 4. After session, friends may become connected on the app.
- Alternatives:
  - A friend declines → match cancelled; inviter notified.

3) Random matching (one-time-per-day)

- Actors: User seeking random match
- Context: User uses daily random match with optional filters.
- Main flow:
  1. Open Random Match, set filters (age, location, interests).
  2. System finds a match using profile and AI scoring and creates a temporary chat.
  3. After chat, users rate the interaction.
- Alternatives:
  - No match found → suggest broadening filters or try later.
  - User already used daily match → show cooldown notice.

4) Chatting, rating and matching quality

- Actors: Matched users
- Context: After a match, a chat is created.
- Main flow:
  1. Users chat in a simple text-only session.
  2. Each user rates the other and may leave feedback.
  3. Ratings influence future matching priority and AI tuning.

5) Privacy management and algorithm-only fields

- Actors: Any user
- Context: User wants certain fields hidden but used for matching.
- Main flow:
  1. User marks fields as Private (algorithm-only) during edit.
  2. App uses those fields for matching but hides them from profiles.
  3. User can change visibility later.

6) Reporting and moderation flow

- Actors: Reporter (user), Moderator/Admin
- Context: Harassment or inappropriate behavior occurs.
- Main flow:
  1. Reporter files report from chat or profile.
  2. System logs report; high-severity reports may suspend accounts pending review.
  3. Moderator reviews and takes action; reporter receives status update.

---

### User Stories

Below are user stories derived from the scenarios with acceptance criteria and priorities.

1) As a new user, I want to create a profile with visibility controls so that the matching algorithm can find relevant people while I control what others see.
Acceptance criteria:
- User can sign up with email/phone.
- User can add profile fields and set visibility to Public / Friends-only / Private (algorithm-only).
- Profile can be created with only required fields; optional fields may be skipped.
Priority: High

2) As an inviter, I want to anonymously match two friends so that they can be introduced without revealing my identity.
Acceptance criteria:
- Inviter can select two friends who are not connected.
- System creates an anonymous two-person chat visible only to the matched friends.
- Inviter is not shown in the chat; friends can opt out.
Priority: High

3) As a user, I want one random match per day with adjustable filters so that I can discover new people similar to me without overuse.
Acceptance criteria:
- User can request a random match once per 24 hours.
- User may set filters (age range, location radius, gender, interests).
- If no match is found, the app suggests adjustments.
Priority: High

4) As a matched participant, I want to rate the interaction so that future matches improve and low-quality behavior is discouraged.
Acceptance criteria:
- Users can rate matched sessions on a simple scale (e.g., 1-5).
- Ratings affect matching priority and are stored in user history.
- System prompts users who don't rate once.
Priority: Medium

5) As a privacy-conscious user, I want to mark certain profile fields as algorithm-only so that the system can use them without exposing them publicly.
Acceptance criteria:
- Profile edit UI includes visibility selector for each field with an option 'Private — used for matching only'.
- Fields marked private are not visible on other users' profiles.
Priority: Medium

6) As a user, I want to report abusive behavior so that moderators can review and act on violations.
Acceptance criteria:
- A 'Report' action exists in chats and profiles.
- Reporter can choose a reason and add details.
- System logs the report and notifies moderators; urgent reports trigger temporary suspension.
Priority: High

7) As a casual user, I want a lightweight feed of brief updates so that I can check friends' activity without heavy engagement.
Acceptance criteria:
- Feed shows short updates from friends and occasional suggested profiles.
- Feed does not prioritize addictive mechanisms; users can ignore it.
Priority: Low

### Diagrame C4

![Context diagram](./images/context_diagram.jpeg)
![Container diagram](./images/container_diagram.jpeg)