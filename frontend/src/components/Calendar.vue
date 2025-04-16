<template>
    <div class="calendar-container">
        <h1>Your Google Calendar Events</h1>

        <div v-if="loading">Loading events...</div>
        <div v-else-if="error">{{ error }}</div>
        <div v-else>
            <div v-if="events.length === 0">
                No upcoming events found.
            </div>
            <ul v-else class="events-list">
                <li v-for="event in events" :key="event.id" class="event-item">
                    <div class="event-title">{{ event.summary }}</div>
                    <div class="event-time">
                        {{ formatDate(event.start.dateTime || event.start.date) }}
                    </div>
                </li>
            </ul>
        </div>
    </div>
</template>

<script>
import axios from 'axios';

export default {
    name: 'CalendarView',
    data() {
        return {
            events: [],
            loading: true,
            error: null
        };
    },
    async mounted() {
        try {
            const response = await axios.get('https://localhost:5047/api/calendar/events');
            this.events = response.data;
        } catch (err) {
            this.error = 'Failed to load calendar events: ' + (err.response?.data || err.message);
        } finally {
            this.loading = false;
        }
    },
    methods: {
        formatDate(dateString) {
            const date = new Date(dateString);
            return date.toLocaleString();
        }
    }
}
</script>

<style scoped>
.calendar-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
}

.events-list {
    list-style: none;
    padding: 0;
    margin: 20px 0;
}

.event-item {
    border: 1px solid #eee;
    border-radius: 4px;
    padding: 15px;
    margin-bottom: 10px;
}

.event-title {
    font-weight: bold;
    font-size: 18px;
    margin-bottom: 5px;
}

.event-time {
    color: #666;
}
</style>
