<template>
  <div className="input-box">
    <input @keyup.enter="findJobs()" @input="changeSpeciality($event.target.value)" type="text" placeholder="Enter Spiciality">
    <input @keyup.enter="findJobs()" @input="changeArea($event.target.value)" type="text" placeholder="Enter City">
    <button className="find-btn" @click="findJobs()">Find Job</button>
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

  .find-btn {
      font-weight: bold;
      border-radius: 20%;
      background-color: aqua;
    }

  .find-btn:hover {
    cursor: pointer;
  }

  .find-btn:active {
    background-color: rgba(0, 255, 255, 0.616);
  }
</style>
