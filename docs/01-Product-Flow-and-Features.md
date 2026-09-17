# CoachApp — Product Flow & Features

> **Audience:** non-technical. This document explains *what the app does* and *how a user
> moves through it* — from login to the last step — for both the **Coach** and the **Trainee**.
> Use it to demo the product or explain it to a client.

---

## 1. What CoachApp is (in one paragraph)

CoachApp is an online platform that lets a **fitness coach run their whole coaching business from
one place**, and lets each of their **trainees follow their program from their phone**. The coach
builds a library of exercises and foods, creates personalized workout and nutrition plans, assigns
them to trainees, and follows each trainee's progress. The trainee logs in, sees exactly what to do
and eat today, logs what they actually did, records their body measurements, and watches their
progress over time. The coach and trainee are always looking at the same data from two sides.

---

## 2. The two kinds of users

| | **Coach** | **Trainee** |
|---|---|---|
| Who they are | The professional running the program | The client being coached |
| What they do | Build & assign plans, manage clients, track results | Follow plans, log workouts/meals, track own progress |
| How many | **One coach per workspace** | **Many trainees per coach** |
| Account created by | Set up when the workspace is created | **Created by the coach** inside the app |

### Each coach gets a private, isolated workspace
Every coach operates in their **own separate workspace** (technically a "tenant"). One coach's
trainees, exercises, foods, plans, and logs are **completely invisible to any other coach**. There
is no shared pool — each coach's world is sealed off. A workspace has exactly **one coach** (the
system actively prevents a second coach from being added to the same workspace) and as many
trainees as the coach adds.

---

## 3. Signing in

Both coaches and trainees use the **same login screen** — a username and password. The system
recognizes who they are and shows them the correct experience:

- A **coach** signs in and lands in the management experience (clients, plans, libraries, tracking).
- A **trainee** signs in and lands in their personal experience (today's plan, logging, progress).

**How a trainee gets their login:** the trainee does **not** sign themselves up. The **coach creates
the trainee's account** inside the app (username + a starting password), then shares those
credentials with the trainee. The coach can **reset a trainee's password** at any time, and can
**deactivate** a trainee — a deactivated trainee can no longer sign in, but all their history is kept
and they can be reactivated later.

---

## 4. The Coach's journey (login → last step)

Think of the coach's work in four stages: **build the libraries → add clients → assign plans →
track results.**

### Stage 1 — Build the reusable libraries
Before assigning anything, the coach stocks two shared libraries that everything else is built from:

- **Exercise library** — each exercise has a name, the muscle group it targets, the equipment it
  needs, plus optional description, instructions, and a photo/video link.
- **Food library** — each food has its nutrition facts for one serving (calories, protein, carbs,
  fat) plus the serving size and unit (e.g. "100 g").

These libraries are reused everywhere, so the coach builds them once. If an exercise or food is
already used inside a plan or a log, the app **won't let it be deleted** (to protect history) — the
coach deactivates it instead so it stops appearing in new plans.

### Stage 2 — Add trainees (clients)
The coach adds each trainee as a profile: name, contact info, gender, birth date, **training goal**
(e.g. lose weight, build muscle, strength…), and starting numbers (height, start weight, target
weight). Creating the trainee also **creates their login account** in one step. The coach can search
and filter their client roster (by name, goal, active/inactive).

### Stage 3 — Build & assign plans
The coach creates two kinds of plans for each trainee:

**Workout plans** — a plan is organized as **Plan → Days → Exercises**:
- The plan contains several training **days** (e.g. "Push Day", "Leg Day").
- Each day can be **scheduled to a weekday** (e.g. Leg Day = every Monday). This weekly schedule is
  what drives the trainee's "Today" screen. A weekday with nothing scheduled is a **rest day**.
- Each exercise on a day carries the prescription: sets, reps, weight, rest, and notes.

**Nutrition plans** — organized as **Plan → Meals → Items**:
- The plan contains **meals** (e.g. "Breakfast", "Lunch").
- Each meal lists **food items** with a quantity (number of servings).
- The coach can set **daily targets** (calories, protein, carbs, fat) for the plan, or let the app
  compute the target automatically from the meals.

**Templates & reuse — the coach doesn't rebuild from scratch each time:**
- The coach can save any workout or nutrition plan as a **reusable template**.
- The coach can **clone a template onto a trainee** to instantly create a personalized plan, then
  tweak it.

**Activating a plan:** a trainee can have several plans on file, but **only one active workout plan
and one active nutrition plan at a time**. Activating a new plan automatically deactivates the
previous one. The trainee only ever sees their **active** plans.

### Stage 4 — Track results
Once a trainee is following their plans, the coach follows their progress:

- **Workout & nutrition logs** — the coach can view every session and every day of eating the
  trainee has logged, filtered by date range.
- **Progress entries** — a dated timeline of body metrics: weight, body-fat %, and measurements
  (chest, waist, hips, arm, thigh). The coach can record these at check-ins; the trainee can also
  record their own.
- **Dashboard analytics** — the coach sees computed stats per trainee:
  - **Nutrition adherence** — how closely the trainee hit their calorie/macro targets, for a day or
    across a range. Going over target stays visible (it's not hidden or capped at 100%).
  - **Workout completion** — how many planned sessions the trainee actually completed over a period.
- **Notes** — the coach writes dated notes for a trainee (feedback, reminders). The trainee can read
  the notes addressed to them.

---

## 5. The Trainee's journey (login → last step)

The trainee experience is designed around **one daily loop**: *open the app → see today → do it →
log it → check progress.*

### Step 1 — "Today" screen (the home screen)
When the trainee opens the app, the **Today** screen answers four questions at a glance:

1. **What do I train today?** — the workout day(s) scheduled for today's weekday, with every
   exercise and its prescribed sets/reps/weight. If nothing is scheduled today, it shows a **rest
   day**.
2. **What do I eat today?** — today's nutrition plan with its meals and targets.
3. **How is today tracking?** — a live nutrition adherence readout for the day.
4. **Have I already logged it?** — whether a workout and/or a nutrition log already exists for today.

### Step 2 — View the full plans
The trainee can open their **active workout plan** and **active nutrition plan** in full detail at
any time — all days, all exercises, all meals. (Older, inactive plans are hidden from the trainee.)

### Step 3 — Log the workout
When the trainee finishes training, they log it. Two ways:

- **Log this session (from the plan)** — one tap seeds a log from today's scheduled day. Every
  prescribed exercise is pre-filled, so the trainee only adjusts what actually happened (e.g. they
  lifted more or fewer reps). The app **remembers what was prescribed vs what was actually done**, so
  the planned-vs-achieved comparison is preserved even if the coach edits the plan later.
- **Manual log** — the trainee logs a free-form session that isn't tied to a plan.

The trainee can edit a log afterward to fix the numbers.

### Step 4 — Log the nutrition
Same idea for food:

- **Log from plan** — seeds today's log from the plan's meals, so the day starts pre-filled and the
  trainee just adjusts.
- **Manual log** — the trainee logs foods and quantities freely.

As foods are logged, the app tallies calories and macros and compares them to the day's target.

### Step 5 — Record progress
The trainee records **progress entries** — body weight, body-fat %, and measurements — on whatever
dates they weigh/measure themselves. This builds their personal progress timeline. (The coach can
also add entries; the trainee can see the coach's entries but can only edit or delete the ones they
recorded themselves.)

### Step 6 — Check the stats (My Dashboard)
The trainee sees their own analytics:

- **Nutrition adherence** — how close they are to their calorie/macro targets today, and their
  weekly average (measured only across the days they actually logged, so a missed day doesn't unfairly
  crush the number).
- **Workout completion** — how many planned sessions they completed this period vs how many were
  planned.

### Step 7 — Profile & coach notes
- The trainee can read the **notes** their coach wrote for them.
- The trainee can update a **limited** part of their profile — phone, email, birth date. Everything
  the coach owns (their goal, target weight, plan assignments, active status, and login name) stays
  under the coach's control.

---

## 6. Feature summary

### Coach can…
| Area | Capability |
|---|---|
| Clients | Add trainees (creates their login), edit, deactivate/reactivate, reset password, search/filter roster |
| Exercise library | Full manage; deletion blocked while in use (deactivate instead) |
| Food library | Full manage (with per-serving nutrition); deletion blocked while in use |
| Workout plans | Build Plan→Days→Exercises, schedule days to weekdays, set the active plan |
| Nutrition plans | Build Plan→Meals→Items, set daily macro targets, set the active plan |
| Templates | Save any plan as a reusable template; clone a template onto a trainee |
| Tracking | View trainee workout & nutrition logs by date range |
| Progress | Record & manage a trainee's body-metrics timeline |
| Dashboards | Per-trainee nutrition adherence & workout completion analytics |
| Notes | Write dated notes for a trainee |

### Trainee can…
| Area | Capability |
|---|---|
| Today | See today's scheduled workout, today's nutrition plan, live adherence, and what's already logged |
| Plans | View their active workout & nutrition plans in full detail |
| Workout logs | Log from the plan (pre-filled) or manually; edit their logs |
| Nutrition logs | Log from the plan (pre-filled) or manually; edit their logs |
| Progress | Record & manage their own body-metrics entries |
| Dashboard | See their own nutrition adherence & workout completion stats |
| Notes | Read notes their coach wrote for them |
| Profile | Update limited contact details (phone, email, birth date) |

---

## 7. A day in the life (narrative)

**The coach, Sunday evening.** Sara logs in, opens her client Ahmed, and clones her "Hypertrophy
Block A" workout template onto him. She schedules Push to Monday/Thursday, Pull to Tuesday/Friday,
Legs to Wednesday/Saturday, and activates the plan. She clones her "2,400 kcal cut" nutrition
template onto him too, confirms the targets, and activates it. She leaves a note: "Start light this
week, focus on form."

**The trainee, Monday morning.** Ahmed opens the app. **Today** shows "Push Day" with his exercises
and prescribed sets/reps, plus his meals and a calorie target. He trains, taps **Log this session**,
adjusts a couple of weights, and saves. Through the day he logs his meals from the plan. In the
evening he records his morning weight as a progress entry and checks **My Dashboard** — he's at 98%
of his calorie target and 1/1 workouts done today.

**The coach, Wednesday.** Sara opens Ahmed's dashboard, sees his week's adherence and completion
trending well, reviews his logged weights on his progress timeline, and adds a note for the next
check-in. Same data, two sides.

---

## 8. Languages

The product ships with full **English and Arabic** support — all screens' labels, roles, and even
error messages are translated in both languages, so it can be presented to Arabic- or
English-speaking clients out of the box.
