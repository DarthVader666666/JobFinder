<template>
<div className="input-box">
    <input @keyup.enter="findJobs()" @input="changeSpeciality($event.target.value)" type="text" placeholder="Enter spiciality">
    <input @keyup.enter="findJobs()" @input="changeArea($event.target.value)" type="text" placeholder="Enter area">
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
        type:Function,
        required:true
    }
}
}
</script>

<style>
.input-box {
    display: flex;
    flex-direction: column;
    align-content: center;
    padding: px;
}

input {
    width: inherit;
    margin-bottom: 0.5rem;
}
</style>