<template>
  <CriteriaInput :changeSpeciality="changeSpeciality" :changeArea="changeArea"></CriteriaInput>
  <button @click="findJobs()">Find Job</button>

  <h3 v-if="loading">Loading...</h3>

  <div v-for="(el, index) in jobs" :key="index">
    <a :href="el['link']" target="_blank">{{el['title']}}</a>
  </div>
</template>

<script>
import CriteriaInput from './components/CriteriaInput.vue'

export default {
components: {
  CriteriaInput
},

  data() {
    return {
      jobs: [],
      url: 'https://www.linkedin.com/jobs/search?',//'https://rabota.by/search/vacancy?',
      speciality: '',
      area: '',
      loading: false
    }
  },

  methods: {
    async findJobs() {
      this.jobs = [];      
      this.loading = true;
      
      this.jobs = await fetch(
        'https://localhost:7150/Jobs/GetList',
        {
          body: JSON.stringify({
            url: this.url,
            speciality: this.speciality,
            area: this.area
          }),
          method: 'POST',
          headers: {
            'Content-Type' : 'application/json'            
          }
        }
      ).then(response => response.json());

      this.loading = false;
    },

    changeSpeciality(value) {
      this.speciality = String(value).trim();
    },

    changeArea(value) {
      this.area = String(value).trim();
    }
  }  
}
</script>

<style scoped>
  a {
    color:white;
  }

  a:hover {
    color:gray;
  }

  h3 {
    color:white;
    font-weight: bold;
  }
</style>
