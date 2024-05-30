<template>
  <button @click="findJobs()">Find Job</button>
  <p>{{ jobs }}</p>
</template>

<script>
export default {
  data() {
    return {
      jobs: ''
    }
  },

  methods: {
    findJobs() {
      var result = fetch(
        'https://rabota.by/vacancies/net-developer',
        //'https://www.google.com/search?q=.net+%D0%B2%D0%B0%D0%BA%D0%B0%D0%BD%D1%81%D0%B8%D0%B8',
        {
          mode: 'no-cors',
          headers: {
            'Access-Control-Allow-Origin':'*'
          }
        }
      )
      .then(result => {
        console.log(result);
      })
    }
  }
}
</script>

<style scoped>

</style>
