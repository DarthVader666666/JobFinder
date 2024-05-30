<template>
  <button @click="findJobs()">Find Job</button>
  <div v-for="(el, index) in jobs" :key="index">
    <a :href="el['url']">{{index}}</a>
  </div>
</template>

<script>
export default {
  data() {
    return {
      jobs: []
    }
  },

  methods: {
    async findJobs() {
      const url = 'https://rabota.by/search/vacancy';
      const speciality = '.net';
      const area = '1002';
      const body = {
            url: `${url}`,
            speciality: `${speciality}`,
            area: `${area}`
          }

      this.jobs = await fetch(
        `https://localhost:7150/Jobs/GetList`,
        {
          body: JSON.stringify(body),
          mode: 'cors',
          method: 'POST',
          headers: {
            'Content-Type' : 'application/json'            
          }
        }
      ).then(response => response.json());
    }
  }
}
</script>

<style scoped>

</style>
