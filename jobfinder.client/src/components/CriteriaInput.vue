<template>
  <div className="input-box">
    <input @keyup.enter="findJobs()" @input="changeSpeciality($event.target.value)" type="text" placeholder="Enter spiciality">
    <input @keyup.enter="findJobs()" @input="changeArea($event.target.value)" type="text" placeholder="Enter area">
    <button @click="findJobs()">Find Job</button>
  </div>

</template>
<script>
  export default {
    props: {
      changeSpeciality: {
        type: Function,
        required: true
      },
      changeArea: {
        type: Function,
        required: true
      },
      findJobs: {
        type: Function,
        required: true
      }
    }
  }
</script>

<style>
  .input-box {
    display: flex;
    width: 150px;
    flex-direction: column;
    justify-items: start;
    margin-bottom: 10px;
  }

  input {
    width: inherit;
    margin-bottom: 0.5rem;
  }
</style>
